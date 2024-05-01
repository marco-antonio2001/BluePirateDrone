import csv
import matplotlib.pyplot as plt

def analyseData(data_file):
    # Define the file path and data container
    data = []

    # Read the CSV data
    with open(data_file, 'r') as file:
        reader = csv.reader(file)
        next(reader)  # Skip header row (assuming it exists)
        for row in reader:
            data.append([float(val) for val in row])  # Convert values to float

    # Extract individual values and create subplots
    fig, axes = plt.subplots(1, 2, figsize=(10, 6))  # Create 2x2 grid of subplots

    # Separate data for roll and pitch
    roll_data, pitch_data, roll_setpoint, pitch_setpoint = zip(*data)

    # Plot each value on a separate subplot

    # Plot roll and roll setpoint
    axes[0].plot(roll_data, label="Roll")
    axes[0].plot(roll_setpoint, label="Roll Setpoint")
    axes[0].set_title("Roll")
    axes[0].legend() 

    # Plot pitch and pitch setpoint
    axes[1].plot(pitch_data, label="Pitch")
    axes[1].plot(pitch_setpoint, label="Pitch Setpoint")
    axes[1].set_title("Pitch")
    axes[1].legend() 

    # Adjust layout and display the figure
    fig.suptitle("Data from " + data_file, fontsize=12)
    plt.tight_layout()
    plt.show()
    