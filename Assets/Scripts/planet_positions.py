import socket
import time
import json
from astropy.time import Time
from astropy.coordinates import get_body_barycentric, SkyCoord, solar_system_ephemeris
from astropy.coordinates import EarthLocation, GCRS
import astropy.units as u

HOST = '127.0.0.1'
PORT = 65432

TIME_STEP_SECONDS = 3600 * 0.005
UPDATE_INTERVAL = 0.01

bodies = ['sun', 'mercury', 'venus', 'earth', 'mars',
          'jupiter', 'saturn', 'uranus', 'neptune', 'moon']

extra_objects = {
    "Polaris": SkyCoord.from_name('Polaris'),
    "Antares": SkyCoord.from_name('Antares'),
    "Capella": SkyCoord.from_name('Capella'),
    "Spica": SkyCoord.from_name('Spica'),
}

# Ankara koordinatları
ankara_location = EarthLocation(lat=39.9334*u.deg, lon=32.8597*u.deg, height=1000*u.m)

def get_positions(t):
    positions = {}
    solar_system_ephemeris.set('builtin')

    for body in bodies:
        coord = get_body_barycentric(body, t)
        positions[body] = {
            'x': coord.x.to(u.km).value / 1e6,
            'y': coord.y.to(u.km).value / 1e6,
            'z': coord.z.to(u.km).value / 1e6
        }

    star_display_distance_scale = 10000.0
    for name, coord in extra_objects.items():
        icrs = coord.icrs.cartesian
        norm = icrs.norm()
        if norm.value == 0:
            positions[name] = {'x':0.0,'y':0.0,'z':0.0}
            continue
        positions[name] = {
            'x': (icrs.x / norm).value * star_display_distance_scale,
            'y': (icrs.y / norm).value * star_display_distance_scale,
            'z': (icrs.z / norm).value * star_display_distance_scale
        }

    earth_coord = get_body_barycentric('earth', t)
    ankara_gcrs = ankara_location.get_itrs(obstime=t).transform_to(GCRS(obstime=t))
    ankara_bary = earth_coord + ankara_gcrs.cartesian
    positions['ankara'] = {
        'x': ankara_bary.x.to(u.km).value / 1e6,
        'y': ankara_bary.y.to(u.km).value / 1e6,
        'z': ankara_bary.z.to(u.km).value / 1e6
    }

    return {
        'timestamp_utc': t.iso,
        'positions': positions
    }

def main():
    t = Time.now()

    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.bind((HOST, PORT))
    s.listen(1)
    print(f"Server listening on {HOST}:{PORT}")

    conn, addr = s.accept()
    print(f"Connected by {addr}")

    try:
        while True:
            data = get_positions(t)
            msg = json.dumps(data) + "\n"
            conn.sendall(msg.encode('utf-8'))
            t += TIME_STEP_SECONDS * u.s
            time.sleep(UPDATE_INTERVAL)
    except KeyboardInterrupt:
        print("Server shutting down due to KeyboardInterrupt")
    except Exception as e:
        print(f"An error occurred: {e}")
    finally:
        if conn:
            conn.close()
            print("Connection closed.")
        if s:
            s.close()
            print("Server socket closed.")

if __name__ == "__main__":
    main()