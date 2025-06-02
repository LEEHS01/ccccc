import numpy as np
import matplotlib.pyplot as plt

t = np.linspace(0, 1, 1000)
original_data = 1.2 * np.sin(2* np.pi * 13 * t + 25152156) 
original_data += 0.5 * np.cos(2* np.pi * 29 * t + 31352315)
original_data += -0.7 * np.sin(2* np.pi * 17 * t + 23146879);
original_data += 0.7 * np.cos(2* np.pi * 39 * t + 1342354678);
noised_data = original_data + np.random.normal(0, 0.2, size=t.shape)
noised_data += 1.0 * np.cos(2* np.pi * 41 * t + 12345);
noised_data += -0.6 * np.cos(2* np.pi * 72 * t + 26261);

F = np.fft.fft(noised_data)
freqs = np.fft.fftfreq(len(t), d=0.001)

H = np.zeros_like(F)
H[(freqs > -40) & (freqs < 40)] = 1  # 0~40Hz만 통과하는 LPF

fft_filtered = F * H
filtered_data = np.fft.ifft(fft_filtered).real


plt.figure(figsize=(10, 5))
plt.plot(original_data, label='original signal', color='red', alpha=1)
plt.plot(noised_data, label='Noised signal', color='gray', alpha=0.6)
plt.plot(filtered_data, label='Filtered signal', color='blue', alpha=0.6)
plt.title('Fourier Transform of Mixed Frequency Signal')
plt.xlabel('Time')
plt.ylabel('Amplitude')
plt.grid(True)
plt.xlim(0, 1000)  # 100Hz까지 보기
plt.show()
