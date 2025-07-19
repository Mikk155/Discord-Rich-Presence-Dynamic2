# Discord-Rich-Presence-Dynamic
Custom discord rich presence "dynamic" that will change on a specified interval of time

# json

Open [drpc.json](drpc.json) to configure

| Key  | Description |
|---|---|
| client_id | Your application ID |
| update_interval | Time (In seconds) at wich the RPC will be updated |
| description | Description to show |
| state | State to show |
| buttons | Button labels (See bellow) |

# button

| Key  | Description |
|---|---|
| title | String to display within the button |
| url | Link to open when click the button |
| image | Imagen name to use for this button (Add to your Application) |
| description | String to display above the button |
