import numpy as np
import matplotlib.pyplot as plt

# 1. 시간축 (x축) 생성: 0부터 2파이까지 0.01 간격
x = np.arange(0, 1, 0.001)

# 2. 사인 함수 생성
# 2. 여러 주파수의 사인파 합성
f1, f2, f3 = 8, 26, 61  # 주파수들 [Hz]
y = (
    1.0 * np.sin(2 * np.pi * f1 * x) +  # 5Hz, 세기 1.0
    0.5 * np.sin(2 * np.pi * f2 * x) +  # 20Hz, 세기 0.5
    0.2 * np.sin(2 * np.pi * f3 * x)    # 60Hz, 세기 0.2
)

# 3. 그래프 그리기
plt.plot(x, y, label='sin(x)', color='blue')  
plt.title('Sine Wave')
plt.xlabel('x')
plt.ylabel('sin(x)')
plt.grid(True)
plt.legend()
plt.show()
