#!/bin/bash

# AttackerRL 훈련 (BT Defender와 대전)
echo "Starting AttackerRL Training against BT Defender..."
mlagents-learn config/AttackerRL_config.yaml --env=AiGameProgramming --run-id=AttackerRL_vs_BTDefender --no-graphics

echo "AttackerRL Training completed!"
echo ""
echo "To resume training, use:"
echo "mlagents-learn config/AttackerRL_config.yaml --env=AiGameProgramming --run-id=AttackerRL_vs_BTDefender --resume"
echo ""
echo "To view tensorboard:"
echo "tensorboard --logdir results"