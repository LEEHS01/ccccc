import numpy as np
import matplotlib.pyplot as plt
from scipy.signal import butter, lfilter, filtfilt

# 1. 샘플링 정보
fs = 1000
t = np.linspace(0, 1, fs, endpoint=False)

# 2. 입력 신호 (저주파 + 고주파 섞임)
x = np.sin(2*np.pi*10*t) + 0.5*np.sin(2*np.pi*120*t)


original_data = 1.2 * np.sin(2* np.pi * 13 * t + 25152156) 
original_data += 0.5 * np.cos(2* np.pi * 29 * t + 31352315)
original_data += -0.7 * np.sin(2* np.pi * 17 * t + 23146879);
original_data += 0.7 * np.cos(2* np.pi * 39 * t + 1342354678);
noised_data = original_data + np.random.normal(0, 0.2, size=t.shape)
noised_data += 1.0 * np.cos(2* np.pi * 41 * t + 12345);
noised_data += -0.6 * np.cos(2* np.pi * 72 * t + 26261);


x = original_data


# 3. Butterworth 필터 설계
order = 4
cutoff = 40  # Hz
b, a = butter(order, cutoff / (fs / 2), btype='low')

print("b : ", b)
print("a : ", a)

# 4-1. 일반 IIR 필터 적용 (지연 포함)
y1 = lfilter(b, a, x)

# 4-2. 위상 보정 필터 적용 (filtfilt: forward + reverse)
y2 = filtfilt(b, a, x)


def my_filtfilt(b, a, x):
    # 1. forward filtering
    y_forward = lfilter(b, a, x)
    
    # 2. reverse the filtered signal
    y_rev = y_forward[::-1]
    
    # 3. backward filtering (same filter, reversed data)
    y_back = lfilter(b, a, y_rev)
    
    # 4. reverse again to restore original time order
    y_final = y_back[::-1]
    
    return y_final

y3 = my_filtfilt(b, a, x)

# 5. 시각화
plt.figure(figsize=(10, 5))
# plt.plot(t, original_data, label='Target Signal(0~40)', alpha=0.5)
# plt.plot(t, noised_data, label='Noisy Original(0~1000)', alpha=0.5)
# plt.plot(t, y1, label='Filtered (lfilter, with delay)', linestyle='dashed')
plt.plot(t, y2, label='Filtered (filtfilt, delay corrected)', linewidth=2)
plt.plot(t, y3, label='Filtered (my_filtfilt, delay corrected)', linewidth=2)
plt.legend()
plt.title('IIR Filtering with and without Phase Delay Compensation')
plt.xlabel('Time [s]')
plt.ylabel('Amplitude')
plt.grid(True)
plt.tight_layout()
plt.show()
