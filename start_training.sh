#!/bin/bash

# ML-Agents Training Script for RL Combat System
# This script starts training for different agent types

# Default values
CONFIG_FILE=""
RUN_ID=""
BEHAVIOR=""
RESUME=""
FORCE=""

# Function to display usage
usage() {
    echo "Usage: $0 [OPTIONS]"
    echo "Options:"
    echo "  -c, --config      Training configuration file (required)"
    echo "  -r, --run-id      Run ID for this training session"
    echo "  -b, --behavior    Specific behavior to train (AttackerAgent or DefenderAgent)"
    echo "  --resume          Resume training from existing run"
    echo "  --force           Force restart training"
    echo "  -h, --help        Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 -c attacker_training_config.yaml -r attacker_run_1"
    echo "  $0 -c defender_training_config.yaml -r defender_run_1"
    echo "  $0 -c selfplay_training_config.yaml -r selfplay_run_1"
    echo "  $0 --resume -r existing_run_id"
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--config)
            CONFIG_FILE="$2"
            shift 2
            ;;
        -r|--run-id)
            RUN_ID="$2"
            shift 2
            ;;
        -b|--behavior)
            BEHAVIOR="$2"
            shift 2
            ;;
        --resume)
            RESUME="--resume"
            shift
            ;;
        --force)
            FORCE="--force"
            shift
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            usage
            exit 1
            ;;
    esac
done

# Check if config file is provided (unless resuming)
if [[ -z "$CONFIG_FILE" && -z "$RESUME" ]]; then
    echo "Error: Configuration file is required"
    usage
    exit 1
fi

# Check if run ID is provided
if [[ -z "$RUN_ID" ]]; then
    echo "Error: Run ID is required"
    usage
    exit 1
fi

# Set default config path
if [[ ! -z "$CONFIG_FILE" ]]; then
    CONFIG_PATH="Assets/RL/config/$CONFIG_FILE"
    
    # Check if config file exists
    if [[ ! -f "$CONFIG_PATH" ]]; then
        echo "Error: Configuration file not found: $CONFIG_PATH"
        exit 1
    fi
fi

# Create results directory if it doesn't exist
mkdir -p results

# Build the mlagents-learn command
CMD="mlagents-learn"

if [[ ! -z "$CONFIG_FILE" ]]; then
    CMD="$CMD $CONFIG_PATH"
fi

CMD="$CMD --run-id=$RUN_ID"

if [[ ! -z "$RESUME" ]]; then
    CMD="$CMD $RESUME"
fi

if [[ ! -z "$FORCE" ]]; then
    CMD="$CMD $FORCE"
fi

if [[ ! -z "$BEHAVIOR" ]]; then
    CMD="$CMD --behavior=$BEHAVIOR"
fi

# Add some additional useful flags
CMD="$CMD --results-dir=results"
CMD="$CMD --num-envs=1"
CMD="$CMD --torch-device=cpu"

echo "Starting ML-Agents training..."
echo "Command: $CMD"
echo "Press Ctrl+C to stop training"
echo "Training logs will be saved to: results/$RUN_ID"
echo ""

# Start training
$CMD
