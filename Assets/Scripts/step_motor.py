import socket
from astropy.coordinates import EarthLocation, AltAz, SkyCoord, get_body
from astropy.time import Time
import astropy.units as u
import time

# Ankara koordinatları
latitude = 39.9334
longitude = 32.8597
elevation = 890
location = EarthLocation(lat=latitude*u.deg, lon=longitude*u.deg, height=elevation*u.m)

# Network ayarları
UDP_IP = "127.0.0.1"
UDP_PORT_RECEIVE = 5005
UDP_PORT_SEND_1 = 5006
UDP_PORT_SEND_2 = 5007
UDP_PORT_SEND_CAMERA = 5008

UPDATE_INTERVAL = 0.5

sock_recv = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock_recv.bind((UDP_IP, UDP_PORT_RECEIVE))
sock_recv.setblocking(False)

sock_send = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

print("Python teleskop sunucusu çalışıyor...")
print(f"Update interval: {UPDATE_INTERVAL}s ({1/UPDATE_INTERVAL:.1f} Hz)")
print(f"Listening on UDP port {UDP_PORT_RECEIVE}")
print(f"Sending to UDP ports {UDP_PORT_SEND_1} and {UDP_PORT_SEND_2} for telescope control and {UDP_PORT_SEND_CAMERA} for camera.")

current_target = None
last_message = ""

def get_target_altaz(target_name):
    """Hedefin azimut ve yükseklik açılarını hesapla"""
    now = Time.now()
    try:
        lower_name = target_name.lower()
        if lower_name in ["sun", "moon", "mercury", "venus", "mars", "jupiter", "saturn", "uranus", "neptune"]:
            coord = get_body(lower_name, now, location)
        else:
            coord = SkyCoord.from_name(target_name)

        altaz_frame = AltAz(obstime=now, location=location)
        altaz_coord = coord.transform_to(altaz_frame)

        if altaz_coord.alt.degree < 0:
            return None, None, "below_horizon"

        return altaz_coord.az.degree, altaz_coord.alt.degree, "visible"
    except Exception as e:
        print(f"Hedef bulunamadı: {e}")
        return None, None, "error"

def main():
    global current_target, last_message
    
    try:
        while True:
            start_time = time.time()
            
            try:
                data, addr = sock_recv.recvfrom(1024)
                new_target = data.decode().strip()
                if new_target != current_target:
                    current_target = new_target
                    print(f"Unity'den yeni hedef alındı: {current_target}")
            except BlockingIOError:
                pass

            if current_target:
                az, alt, status = get_target_altaz(current_target)
                
                if status == "visible":
                    message = f"AZ:{az:.2f} ALT:{alt:.2f}"
                elif status == "below_horizon":
                    message = f"ERROR:Target '{current_target}' below horizon"
                else:
                    message = f"ERROR:Target '{current_target}' not found"

                if message != last_message:
                    print(f"Gönderiliyor: {message}")
                    last_message = message

                try:
                    sock_send.sendto(message.encode(), (UDP_IP, UDP_PORT_SEND_1))
                    sock_send.sendto(message.encode(), (UDP_IP, UDP_PORT_SEND_2))
                    sock_send.sendto(message.encode(), (UDP_IP, UDP_PORT_SEND_CAMERA))
                except Exception as e:
                    print(f"Veri gönderme hatası: {e}")

            elapsed = time.time() - start_time
            sleep_time = max(0, UPDATE_INTERVAL - elapsed)
            time.sleep(sleep_time)
            
    except KeyboardInterrupt:
        print("\nTeleskop sunucusu kapatılıyor...")
    except Exception as e:
        print(f"Bir hata oluştu: {e}")
    finally:
        sock_recv.close()
        sock_send.close()
        print("Socket bağlantıları kapatıldı.")

if __name__ == "__main__":
    main()