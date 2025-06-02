import numpy as np
import matplotlib.pyplot as plt

# 시드 고정 (재현성)
np.random.seed(42)

# 설정
timesteps = 500000
true_position = 100       # 초기 위치
velocity = 1              # 일정 속도 이동
process_noise = 1e-5      # 모델 노이즈 분산 Q
measurement_noise = 10     # 측정 노이즈 표준편차 (R = 16)



t = np.linspace(0, 1, timesteps, endpoint=False)
original_data = 1.2 * np.sin(2* np.pi * 13 * t + 25152156) 
original_data += 0.5 * np.cos(2* np.pi * 29 * t + 31352315)
original_data += -0.7 * np.sin(2* np.pi * 17 * t + 23146879);
original_data += 0.7 * np.cos(2* np.pi * 39 * t + 1342354678);
noised_data = original_data + np.random.normal(0, 0.2, size=t.shape)
# noised_data += 1.0 * np.cos(2* np.pi * 41 * t + 12345);
noised_data += -0.6 * np.cos(2* np.pi * 182 * t + 26261);
# noised_data += 1.6 * np.sin(2* np.pi * 105 * t + 12435657);



# 실제 위치 시퀀스 생성
# x_true = [true_position + velocity * t for t in range(timesteps)]
x_true = original_data


# 측정값: 실제값 + 가우시안 노이즈
# z_measured = [x + np.random.normal(0, measurement_noise) for x in x_true]
z_measured =noised_data;

# 칼만 필터 초기값
x_hat = 0       # 추정값 초기화
P = 1           # 오차 공분산 초기값
Q = process_noise      # 모델 노이즈 공분산
R = measurement_noise**2  # 측정 노이즈 공분산

x_estimates = []

for z in z_measured:
    # 예측 단계
    x_pred = x_hat
    P_pred = P + Q

    # 칼만 이득 계산
    K = P_pred / (P_pred + R)

    # 업데이트 단계
    x_hat = x_pred + K * (z - x_pred)
    P = (1 - K) * P_pred

    # 추정 저장
    x_estimates.append(x_hat)

# 시각화
plt.figure(figsize=(10, 5))
plt.plot(z_measured, label="Measured (Noisy)", linestyle='dotted')
plt.plot(x_true, label="True Position", linewidth=2)
plt.plot(x_estimates, label="Kalman Estimate", linestyle='dashed')
plt.title("1D Kalman Filter Simulation")
plt.xlabel("Time Step")
plt.ylabel("Position")
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.show()
