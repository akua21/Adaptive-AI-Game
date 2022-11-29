import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import csv
import os
# Read the survey from the CSV file in the "Survey" folder
survey = pd.read_csv("Survey/survey.csv")

short_col_names = [
    "date",
    "alias",
    "hours_of_play",
    "genres",
    "game_ability",
    "enjoyment",
    "game_duration",
    "mode",
    "difficult_attack",
    "difficult_defend",
    "difficult_catch",
    "adaptability",
    "general_difficulty",
    "continue_playing",
    "feedback"
]

mode_to_botName = {
    "Mode 1": "botGenetic",
    "Mode 2": "bot",
    "Mode 3": "botMany",
}

survey.columns = short_col_names

# For each mode, load the data from the person playing "alias" from the "Metrics" folder
mode1 = pd.DataFrame()
mode2 = pd.DataFrame()
mode3 = pd.DataFrame()

for alias, mode in zip(survey["alias"], survey["mode"]):
    with open(f"Metrics/{alias}.csv", "r") as file:
        reader = csv.reader(file)

        # Read header first
        header = next(reader)[:-1]
        header.append("alias")
        csv_info = [header]
        for row in reader:
            content = row[:len(header)-1]
            content.append(alias)
            csv_info.append(content)

    # Save as pandas dataframe
    df = pd.DataFrame(csv_info[1:], columns=csv_info[0])
    if mode == "Mode 1":
        mode1 = mode1.append(df)
    elif mode == "Mode 2":
        mode2 = mode2.append(df)
    elif mode == "Mode 3":
        mode3 = mode3.append(df)

# Mode 1 needs cleaning. Remove all calibrating games
mode1 = mode1[mode1["bot"] == mode_to_botName["Mode 1"]]


# Reset index of all three modes
mode1 = mode1.reset_index(drop=True)
mode2 = mode2.reset_index(drop=True)
mode3 = mode3.reset_index(drop=True)

# Create a new column for if player wins, other for if bot winns, and other where the life difference is negative is player loses and positive if player wins
mode1["player_wins"] = mode1["winner"] == "player"
mode1["bot_wins"] = mode1["winner"] == mode_to_botName["Mode 1"]
mode1["life_diff"] = mode1["winnerHP"]
mode1["life_diff"] = mode1["life_diff"].astype(int)
mode1.loc[mode1["bot_wins"] == True, "life_diff"] *= -1

mode2["player_wins"] = mode2["winner"] == "player"
mode2["bot_wins"] = mode2["winner"] == mode_to_botName["Mode 2"]
mode2["life_diff"] = mode2["winnerHP"]
mode2["life_diff"] = mode2["life_diff"].astype(int)
mode2.loc[mode2["bot_wins"] == True, "life_diff"] *= -1

mode3["player_wins"] = mode3["winner"] == "player"
mode3["bot_wins"] = mode3["winner"] == mode_to_botName["Mode 3"]
mode3["life_diff"] = mode3["winnerHP"]
mode3["life_diff"] = mode3["life_diff"].astype(int)
mode3.loc[mode3["bot_wins"] == True, "life_diff"] *= -1

group_mode1 = mode1.groupby("alias")

group1_mean = group_mode1.agg(
    {
        "player_wins": np.mean,
        "life_diff": np.mean,
    })

group_mode2 = mode2.groupby("alias")

group2_mean = group_mode2.agg(
    {
        "player_wins": np.mean,
        "life_diff": np.mean,
    })

group_mode3 = mode3.groupby("alias")

group3_mean = group_mode3.agg(
    {
        "player_wins": np.mean,
        "life_diff": np.mean,
    })

# Add the three modes together and then to the alias
group_mean = group1_mean.append(group2_mean).append(group3_mean)
group_mean = group_mean.reset_index(drop=False)

# Add the survey data to the group_mean
survey = survey.merge(group_mean, on="alias", how="outer")

# Print average of player wins by mode from the survey
print(f"Average player wins by mode: {survey.groupby('mode')['player_wins'].mean()}")

# Print average of life difference by mode from the survey
print(f"Average life difference by mode: {survey.groupby('mode')['life_diff'].mean()}")

# Plot the average of player wins by mode from the survey
survey.groupby("mode")["player_wins"].mean().plot(kind="bar")
plt.title("Average player wins by mode", color="white")

plt.gca().spines["bottom"].set_color("white")
plt.gca().spines["top"].set_color("white")
plt.gca().spines["right"].set_color("white")
plt.gca().spines["left"].set_color("white")
plt.gca().tick_params(axis="x", colors="white")
plt.gca().tick_params(axis="y", colors="white")
# x axis
plt.xlabel("Mode")
plt.gca().set_xlabel(plt.gca().get_xlabel(), weight="bold", color="white")
plt.xticks(rotation=0)
# y axis
plt.ylabel("Average player wins")
plt.gca().set_ylabel(plt.gca().get_ylabel(), weight="bold", color="white")
# Bars colors
for i, bar in enumerate(plt.gca().patches):
    if i == 0:
        bar.set_color("blue")
    elif i == 1:
        bar.set_color("lightblue")
    elif i == 2:
        bar.set_color("darkblue")   
# Save the plot
plt.savefig("./Analysis/Average player wins by mode.png", transparent=True)  
plt.close()

# Plot the average player wins distribution by mode from the survey
survey.groupby("mode")["player_wins"].plot(kind="kde", legend=False)
plt.title("Average player wins distribution by mode", color="white")

plt.gca().spines["bottom"].set_color("white")
plt.gca().spines["top"].set_color("white")
plt.gca().spines["right"].set_color("white")
plt.gca().spines["left"].set_color("white")
plt.gca().tick_params(axis="x", colors="white")
plt.gca().tick_params(axis="y", colors="white")

# x axis
plt.xlabel("Average player wins")
plt.gca().set_xlabel(plt.gca().get_xlabel(), weight="bold", color="white")
# y axis
plt.ylabel("Density")
plt.gca().set_ylabel(plt.gca().get_ylabel(), weight="bold", color="white")
# Line colors
for i, line in enumerate(plt.gca().get_lines()):    
    if i == 0:
        line.set_color("blue")
    elif i == 1:
        line.set_color("lightblue")
    elif i == 2:
        line.set_color("darkblue")
# Change color of the legend
# plt.gca().legend().get_texts()[0].set_color("blue")
# plt.gca().legend().get_texts()[1].set_color("lightblue")
# plt.gca().legend().get_texts()[2].set_color("darkblue")     
# Save the plot
plt.savefig("./Analysis/Average player wins distribution by mode.png", transparent=True)
plt.close()

# Plot the average of life difference by mode from the survey
survey.groupby("mode")["life_diff"].mean().plot(kind="bar")
plt.title("Average life difference by mode", color="white")

plt.gca().spines["bottom"].set_color("white")
plt.gca().spines["top"].set_color("white")
plt.gca().spines["right"].set_color("white")
plt.gca().spines["left"].set_color("white")
plt.gca().tick_params(axis="x", colors="white")
plt.gca().tick_params(axis="y", colors="white")

# x axis
plt.xlabel("Mode")
plt.gca().set_xlabel(plt.gca().get_xlabel(), weight="bold", color="white")
plt.xticks(rotation=0)
# y axis
plt.ylabel("Average life difference")
plt.gca().set_ylabel(plt.gca().get_ylabel(), weight="bold", color="white")
# Bars colors
for i, bar in enumerate(plt.gca().patches):
    if i == 0:
        bar.set_color("magenta")
    elif i == 1:
        bar.set_color("lightpink")
    elif i == 2:
        bar.set_color("darkmagenta")  
# Save the plot
plt.savefig("./Analysis/Average life difference by mode.png", transparent=True)  
plt.close()

# Plot the average of life difference distribution by mode from the survey
survey.groupby("mode")["life_diff"].plot(kind="kde", legend=False)
plt.title("Average life difference distribution by mode", color="white")

plt.gca().spines["bottom"].set_color("white")
plt.gca().spines["top"].set_color("white")
plt.gca().spines["right"].set_color("white")
plt.gca().spines["left"].set_color("white")
plt.gca().tick_params(axis="x", colors="white")
plt.gca().tick_params(axis="y", colors="white")

# x axis
plt.xlabel("Life difference")
plt.gca().set_xlabel(plt.gca().get_xlabel(), weight="bold", color="white")
# y axis
plt.ylabel("Density")
plt.gca().set_ylabel(plt.gca().get_ylabel(), weight="bold", color="white")
# Line colors
for i, line in enumerate(plt.gca().get_lines()):
    if i == 0:
        line.set_color("magenta")
    elif i == 1:
        line.set_color("lightpink")
    elif i == 2:
        line.set_color("darkmagenta")
# Change color of the legend
# plt.gca().legend().get_texts()[0].set_color("magenta")
# plt.gca().legend().get_texts()[1].set_color("lightpink")
# plt.gca().legend().get_texts()[2].set_color("darkmagenta")     
# Save the plot
plt.savefig("./Analysis/Density life difference distribution by mode.png", transparent=True)
plt.close()

# Plot the general_difficulty by mode from the survey
survey.groupby("mode")["general_difficulty"].mean().plot(kind="bar")
plt.title("Average general difficulty by mode", color="white")

plt.gca().spines["bottom"].set_color("white")
plt.gca().spines["top"].set_color("white")
plt.gca().spines["right"].set_color("white")
plt.gca().spines["left"].set_color("white")
plt.gca().tick_params(axis="x", colors="white")
plt.gca().tick_params(axis="y", colors="white")

# x axis
plt.xlabel("Mode")
plt.gca().set_xlabel(plt.gca().get_xlabel(), weight="bold", color="white")
plt.xticks(rotation=0)
# y axis
plt.ylabel("Average general difficulty")
plt.gca().set_ylabel(plt.gca().get_ylabel(), weight="bold", color="white")
# Bars colors
for i, bar in enumerate(plt.gca().patches):
    if i == 0:
        bar.set_color("red")
    elif i == 1:
        bar.set_color("darkred")
    elif i == 2:
        bar.set_color("lightcoral")
# Save the plot
plt.savefig("./Analysis/Average general difficulty by mode.png", transparent=True)  
plt.close()

# Plot the general_difficulty distribution by mode from the survey
survey.groupby("mode")["general_difficulty"].plot(kind="kde", legend=False)
plt.title("General difficulty distribution by mode", color="white")

plt.gca().spines["bottom"].set_color("white")
plt.gca().spines["top"].set_color("white")
plt.gca().spines["right"].set_color("white")
plt.gca().spines["left"].set_color("white")
plt.gca().tick_params(axis="x", colors="white")
plt.gca().tick_params(axis="y", colors="white")

# x axis
plt.xlabel("General difficulty")
plt.gca().set_xlabel(plt.gca().get_xlabel(), weight="bold", color="white")
# y axis
plt.ylabel("Density")
plt.gca().set_ylabel(plt.gca().get_ylabel(), weight="bold", color="white")
# Line colors
for i, line in enumerate(plt.gca().get_lines()):
    if i == 0:
        line.set_color("red")
    elif i == 1:
        line.set_color("darkred")
    elif i == 2:
        line.set_color("lightcoral")
# Change color of the legend
# plt.gca().legend().get_texts()[0].set_color("red")
# plt.gca().legend().get_texts()[1].set_color("darkred")
# plt.gca().legend().get_texts()[2].set_color("lightcoral")   
# Save the plot
plt.savefig("./Analysis/Density general difficulty distribution by mode.png", transparent=True)
plt.close()

# Plot the game_ability by mode from the survey
survey.groupby("mode")["game_ability"].mean().plot(kind="bar")
plt.gca().spines["bottom"].set_color("white")
plt.gca().spines["top"].set_color("white")
plt.gca().spines["right"].set_color("white")
plt.gca().spines["left"].set_color("white")
plt.gca().tick_params(axis="x", colors="white")
plt.gca().tick_params(axis="y", colors="white")
plt.title("Average game ability by mode", color="white")
# x axis
plt.xlabel("Mode", color="white")
plt.gca().set_xlabel(plt.gca().get_xlabel(), weight="bold", color="white")
plt.xticks(rotation=0)
# y axis
plt.ylabel("Average game ability", color="white")
plt.gca().set_ylabel(plt.gca().get_ylabel(), weight="bold", color="white")
# Bars colors
for i, bar in enumerate(plt.gca().patches):
    if i == 0:
        bar.set_color("orange")
    elif i == 1:
        bar.set_color("darkorange")
    elif i == 2:
        bar.set_color("gold")
# Save the plot
plt.savefig("./Analysis/Average game ability by mode.png", transparent=True)
plt.close()

# Plot the game_ability distribution by mode from the survey
survey.groupby("mode")["game_ability"].plot(kind="kde", legend=False)
# Change color of the lines and the legend
for i, line in enumerate(plt.gca().get_lines()):    
    if i == 0:
        line.set_color("orange")
        line.set_label("Mode 1")
    elif i == 1:
        line.set_color("darkorange")
        line.set_label("Mode 2")
    elif i == 2:
        line.set_color("gold")
        line.set_label("Mode 3")
# Change color of the legend
# plt.gca().legend().get_texts()[0].set_color("orange")
# plt.gca().legend().get_texts()[1].set_color("darkorange")
# plt.gca().legend().get_texts()[2].set_color("gold")     

plt.gca().spines["bottom"].set_color("white")
plt.gca().spines["top"].set_color("white")
plt.gca().spines["right"].set_color("white")
plt.gca().spines["left"].set_color("white")
plt.gca().tick_params(axis="x", colors="white")
plt.gca().tick_params(axis="y", colors="white")
plt.title("Game ability distribution by mode", color="white")
# x axis
plt.xlabel("Game ability")
plt.xlim(0, 10)
plt.gca().set_xlabel(plt.gca().get_xlabel(), weight="bold", color="white")
# y axis
plt.ylabel("Density")
plt.gca().set_ylabel(plt.gca().get_ylabel(), weight="bold", color="white")
# Save the plot
plt.savefig("./Analysis/Density game ability distribution by mode.png", transparent=True)
plt.close()
