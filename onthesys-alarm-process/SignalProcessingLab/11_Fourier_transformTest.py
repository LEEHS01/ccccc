import numpy as np
import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation

# 데이터 설정
sampling_rate = 10000
t = np.linspace(0, 100, sampling_rate, endpoint=False)

# 합성 신호
f1, f2, f3 = 8, 26, 61
signal = (
    1.0 * np.sin(2 * np.pi * f1 * t + 14215.1251) +
    0.5 * np.sin(2 * np.pi * f2 * t + 215412521.214214) +
    0.2 * np.sin(2 * np.pi * f3 * t +21366.12421)
)

signalReconstructed = 0*t;

# 주파수 범위
frequencies = np.linspace(1, 100, 1000)

# 애니메이션용 Figure 설정
fig1, ax1 = plt.subplots()
line, = ax1.plot([], [], lw=2)
centroid_point, = ax1.plot([], [], 'ro')  # 무게중심 점
ax1.set_xlim(-2, 2)
ax1.set_ylim(-2, 2)
ax1.set_aspect('equal')
ax1.set_title("Signal × $e^{-2\pi i f t}$ on Complex Plane")
ax1.set_xlabel("Real")
ax1.set_ylabel("Imaginary")
ax1.grid(True)

# L 값 플롯용 Figure 설정
fig2, ax2 = plt.subplots()
ax2.set_xlim(0, 100)
ax2.set_ylim(0, 1)
L_line, = ax2.plot([], [], 'b-')
ax2.set_title("Distance L of Centroid from Origin")
ax2.set_xlabel("Frequency [Hz]")
ax2.set_ylabel("L = |Centroid|")

# 재구축 신호 출력
fig3, ax3 = plt.subplots()
ax3.set_xlim(0, t.max())
ax3.set_ylim(-7, 7)
recon_line, = ax3.plot([], [], 'b-')
ax3.set_title("Reconstructed Signal")
ax3.set_xlabel("time")
ax3.set_ylabel("value")

# 결과 저장용
L_vals = []
recon_vals = []

def init():
    line.set_data([], [])
    centroid_point.set_data([], [])
    L_line.set_data([], [])
    recon_line.set_data([], [])
    return line, centroid_point, L_line, recon_line

def update(frame):
    f = frequencies[frame]
    complexes = signal * np.exp(-2j * np.pi * f * t)

    # 복소 궤적 및 무게중심
    line.set_data(complexes.real, complexes.imag)
    centroid = np.mean(complexes)
    centroid_point.set_data([centroid.real], [centroid.imag])

    # L 계산 및 갱신
    L = np.abs(centroid)
    L_vals.append(L)
    L_line.set_data(frequencies[:frame+1], L_vals)

    recon_vals = np.zeros_like(t)

    if L > 0.05 :
        for i in range(frame + 1):
            A = L_vals[i]  # 진폭 (절대값)
            f_comp = frequencies[i]  # 주파수
            recon_vals += 2 * A * np.cos(2 * np.pi * f_comp * t)  # 위상 생략 근사
        # recon_vals = np.sum L_vals
        recon_line.set_data(t, recon_vals)
        ax1.set_title(f"f = {f:.1f} Hz")
    return line, centroid_point, L_line, recon_line

# 애니메이션 시작
ani = FuncAnimation(fig1, update, frames=len(frequencies), init_func=init,
                    blit=True, interval=10, repeat=False)

plt.show()
