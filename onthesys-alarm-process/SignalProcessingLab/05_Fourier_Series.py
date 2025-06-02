import numpy as np
import matplotlib.pyplot as plt
import matplotlib.animation as animation

# 1. 시간축 정의
t = np.linspace(-np.pi * 2, np.pi * 2, 1000)

# 2. 사각파를 근사하는 푸리에 급수 함수
def fourier_square_wave(t, N_terms):
    y = np.zeros_like(t)
    for n in range(1, N_terms * 2, 2):  # 홀수 항만
        y += (4 / (np.pi * n)) * np.sin(n * t)
    return y

# 3. 애니메이션 설정
fig, ax = plt.subplots()
line, = ax.plot([], [], lw=2)
ax.set_xlim(-np.pi * 2, np.pi * 2)
ax.set_ylim(-1.5, 1.5)
ax.set_title('Fourier Series Approximation of Square Wave')
ax.set_xlabel('t')
ax.set_ylabel('f(t)')
ax.grid(True)

# 4. 초기화 함수
def init():
    line.set_data([], [])
    return line,

# 5. 프레임마다 호출될 업데이트 함수
def update(n):
    y = fourier_square_wave(t, n + 1)
    line.set_data(t, y) # np.sign(np.sin(t))
    ax.set_title(f'N = {n + 1} terms')
    return line,

# 6. 애니메이션 생성
ani = animation.FuncAnimation(fig, update, frames=3, init_func=init,
                              blit=True, interval=300, repeat=True)

plt.show()
