import numpy as np
import matplotlib.pyplot as plt
from scipy.signal import firwin, lfilter

# 1. 샘플링 설정
fs = 1000  # Hz
t = np.arange(0, 1, 1/fs)
x = np.sin(2*np.pi*5*t) + 0.5*np.sin(2*np.pi*120*t)  # 저주파 + 고주파 노이즈


original_data = 1.2 * np.sin(2* np.pi * 13 * t + 25152156) 
original_data += 0.5 * np.cos(2* np.pi * 29 * t + 31352315)
original_data += -0.7 * np.sin(2* np.pi * 17 * t + 23146879);
original_data += 0.7 * np.cos(2* np.pi * 39 * t + 1342354678);
noised_data = original_data + np.random.normal(0, 0.2, size=t.shape)
noised_data += 1.0 * np.cos(2* np.pi * 41 * t + 12345);
noised_data += -0.6 * np.cos(2* np.pi * 72 * t + 26261);


x = noised_data



# 2. FIR 필터 계수 설계 (저역통과, 차단주파수 50Hz)
numtaps = 53  #윈도우 크기
cutoff = 40  # 임계값
b = firwin(numtaps, cutoff, fs=fs)

# 3. 필터 적용 (지연 포함)
y = lfilter(b, 1.0, x)

# 4. 위상 지연 보정
delay = (numtaps - 1) // 2
y_corrected = y[delay:]  # 지연만큼 앞으로 당김
x_trimmed = x[:len(y_corrected)]  # 길이 맞추기

# 5. 시각화
plt.figure(figsize=(10, 5))
plt.plot(t, x, label='Original', alpha=0.5)
# plt.plot(t, y, label='Filtered (with delay)', linestyle='dashed')
plt.plot(t[:len(y_corrected)], y_corrected, label='Filtered (delay corrected)', linewidth=2)
plt.legend()
plt.title('FIR Filtering and Phase Delay Compensation')
plt.xlabel('Time [s]')
plt.ylabel('Amplitude')
plt.grid(True)
plt.tight_layout()
plt.show()