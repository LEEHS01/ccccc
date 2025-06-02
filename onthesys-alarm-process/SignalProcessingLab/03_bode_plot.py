import numpy as np
import matplotlib.pyplot as plt
from scipy import signal

# RC 값 설정
R = 1  # 저항 (Ohm)
C = 1  # 커패시터 (Farad)
RC = R * C

# 전달함수 정의: 1 / (RC*s + 1)
num = [1]
den = [RC, 1]
system = signal.TransferFunction(num, den)

# 주파수 응답 (Bode plot)
w, mag, phase = signal.bode(system)

# Magnitude plot
plt.figure()
plt.semilogx(w, mag)
plt.title('Bode Magnitude Plot')
plt.xlabel('Frequency (rad/s)')
plt.ylabel('Magnitude (dB)')
plt.grid(True)

# Phase plot
plt.figure()
plt.semilogx(w, phase)
plt.title('Bode Phase Plot')
plt.xlabel('Frequency (rad/s)')
plt.ylabel('Phase (degrees)')
plt.grid(True)

plt.show()
