#!/bin/bash

# DefenderRL 훈련 (BT Attacker와 대전)
echo "Starting DefenderRL Training against BT Attacker..."
mlagents-learn config/DefenderRL_config.yaml --env=AiGameProgramming --run-id=DefenderRL_vs_BTAttacker --no-graphics

echo "DefenderRL Training completed!"
echo ""
echo "To resume training, use:"
echo "mlagents-learn config/DefenderRL_config.yaml --env=AiGameProgramming --run-id=DefenderRL_vs_BTAttacker --resume"
echo ""
echo "To view tensorboard:"
echo "tensorboard --logdir results"