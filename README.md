# Steam Playtime Tracker (SPT)
SPT is a open source project designed to show a more detailed view of your Steam playtime. This project is not a live usage tracker (as in records what apps you use for how long whilst you use them) nor is it a game launcher. This is purely for viewing a historic record of your playtime history. Do not expect to have a full record of playtime from when you started playing to present.
This project nor myself have any affiliation to Valve or the Steam marketplace/game launcher.

# Getting Started and Navigating
## Download
Clone the repository to your drive, and build the project; or download the latest release.
## Setting required data
It is simple to get started, find your steam installation and enter the path into the installation input field and press continue!
<img width="1600" height="900" alt="image" src="https://github.com/user-attachments/assets/b1131e24-5f14-40dc-a08c-d377079d6a65" />


## App View
You will be brought to the App View. This shows all local apps installed which also have a **trackable** playtime.
You can sort by Name and Playtime with the buttons at the top left.
Simply scroll and click on any app you wish to view the playtime of.
<img width="1600" height="900" alt="image" src="https://github.com/user-attachments/assets/60c8375c-f449-4d53-a8c4-29b850aab7d2" />

## Playtime View
You will be broght to a view with a few key elements. On the left hand side you have what game you have selected. To the right of that, there is the graph of your playtime - and by default is by month.
Below the graph there are two date select fields, the top field is the **Start Date** and the bottom field is the **End Date**. Simply put your desired date ranges in!
Finally, there is a summary of key stats, and so far there is just total playtime.
<img width="1600" height="900" alt="image" src="https://github.com/user-attachments/assets/65952b9d-1d32-4199-a516-db6c492353b0" />

# What Is Next
With college coming up soon, I will not be updating this as often, but I do intend to finish off graphing by grouping overlapping months by the year that month is in through a legend key. Simply put, say there are tracked playtime between 2022-2025 then the graph will have 3 bars under each month (or as many bars as there can be for months with playtime in them per year) where each bar will have a colour linked to the year.
Additionally, it would be nice to add plug and play modules for custom graphing displays, among other potential options (like custom sorting) - but as of now I have no plan on how to do that.
One more thing of note, this is more of a technical detail, I plan to use alternate methods for gathering playtime of a game as there may be cases where more data can be extracted for playtime.

# Technical Details
## How Is Playtime Even Tracked?
After a brief search, Steam stored some info about apps in logs. Mainly, "gameprocess_log.txt" is used as it provides all you would need to track playtime.
This data is then packed into a struct containing the session length, when the session started, as well as the AppId.
Finally, a list of these structs is used for graphing.
