using DiscordRPC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

enum ButtonList : int
{
    Title = 0,
    URL,
    Image,
    Description
};

class Program
{
    static DiscordRpcClient? client;

    static string Description = "";
    static string State = "";

    static int DisplayButton = 0;

    static int UpdateInterval = 10;

    static List<List<string?>> Buttons = new List<List<string?>>();

    static async Task Main( string[] args )
    {
#if DEBUG
        string ConfigFilePath = Path.Combine( AppContext.BaseDirectory, "../../../drpc.json" );
#else
        string ConfigFilePath = Path.Combine( AppContext.BaseDirectory, "drpc.json" );
#endif

        if( !Path.Exists( ConfigFilePath ) )
        {
            Shutdown( $"File \"{ConfigFilePath}\" not found" );
            return;
        }

        JObject? CFG = JsonConvert.DeserializeObject<JObject>( File.ReadAllText( ConfigFilePath ) );

        if( CFG is null )
        {
            Shutdown( $"Failed to open: {CFG}" );
            return;
        }

        // Custom discord application?
        string DiscordClientID = "1396104818757468261";

        if( CFG.ContainsKey( "client_id" ) )
        {
            JToken? CustomID = CFG.GetValue( "client_id" );

            if( CustomID is not null )
            {
                DiscordClientID = CustomID.ToString();
            }
        }

        if( CFG.ContainsKey( "description" ) )
        {
            JToken? DescriptionJ = CFG.GetValue( "description" );

            if( DescriptionJ is not null )
            {
                Description = DescriptionJ.ToString();
            }
        }

        if( CFG.ContainsKey( "state" ) )
        {
            JToken? StateJ = CFG.GetValue( "state" );

            if( StateJ is not null )
            {
                State = StateJ.ToString();
            }
        }

        if( CFG.ContainsKey( "update_interval" ) )
        {
            JToken? IntervalUpdate = CFG.GetValue( "update_interval" );

            if( IntervalUpdate is not null )
            {
                UpdateInterval = (int)IntervalUpdate;
            }
        }

        if( CFG.ContainsKey( "buttons" ) )
        {
            JToken? ButtonsJ = CFG.GetValue( "buttons" );

            if( ButtonsJ is not null )
            {
                int index = -1;

                foreach( JToken button in ButtonsJ.ToArray() )
                {
                    index++;

                    string? title = button[ "title" ] != null ? button[ "title" ]!.ToString() : null;
                    string? url = button[ "url" ] != null ? button[ "url" ]!.ToString() : null;
                    string? image = button[ "image" ] != null ? button[ "image" ]!.ToString() : null;
                    string? description = button[ "description" ] != null ? button[ "description" ]!.ToString() : null;

                    if( title is null )
                    {
                        Console.WriteLine( $"Ignoring button at index {index} without a title set" );
                        continue;
                    }

                    if( url is null )
                    {
                        Console.WriteLine( $"Ignoring button at index {index} without a url set" );
                        continue;
                    }

                    Buttons.Add( new List<string?>(){ title, url, image, description } );
                }
            }
        }

        client = new DiscordRpcClient( DiscordClientID );

        if( client is null )
        {
            Shutdown( $"Null client. are you sure this is the correct client ID? {DiscordClientID}" );
            return;
        }

        client.Initialize();

        Timestamps StartTime = Timestamps.Now;

        while( true )
        {
            List<Button> ListButtons = new List<Button>();

            Assets assets = new Assets();

            string CurrentDescription = Description;

            if( Buttons.Count > 0 )
            {
                if( DisplayButton == Buttons.Count )
                {
                    DisplayButton = 0;
                }

                List<string?> CurrentButton = Buttons[ DisplayButton ];

                DisplayButton++;

                string? CustomDescription = CurrentButton[ (int)ButtonList.Description ];

                if( CustomDescription is not null )
                {
                    CurrentDescription = CustomDescription;
                }

                string? CustomImage = CurrentButton[ (int)ButtonList.Image ];

                if( CustomImage is not null )
                {
                    assets.LargeImageKey = CustomImage;
                    assets.SmallImageKey = CustomImage;
                }

                Button TopButton = new Button() {
                    Label = CurrentButton[ (int)ButtonList.Title ],
                    Url = CurrentButton[ (int)ButtonList.URL ]
                };

                ListButtons.Add( TopButton );

                // If there's more buttons then display the next one bellow
                if( Buttons.Count > 1 )
                {
                    int NextButtonIndex = DisplayButton;

                    if( DisplayButton == Buttons.Count )
                    {
                        NextButtonIndex = 0;
                    }

                    List<string?> NextButton = Buttons[ NextButtonIndex ];

                    ListButtons.Add( new Button(){
                        Label = NextButton[ (int)ButtonList.Title ],
                        Url = NextButton[ (int)ButtonList.URL ]
                    } );
                }
            }

            try
            {
                RichPresence RPC = new RichPresence() {
                    Assets = assets,
                    Timestamps = StartTime,
                    Buttons = ListButtons.ToArray(),
                    Details = CurrentDescription,
                    State = State
                };

                client.SetPresence( RPC );
            }
            catch( Exception e )
            {
                Console.WriteLine( $"Exception: {e}" );
            }

            await Task.Delay( TimeSpan.FromSeconds( UpdateInterval ) );
        }
    }

    static void Shutdown( string? ErrMessage = null )
    {
        if( ErrMessage is not null )
        {
            Console.WriteLine( ErrMessage );
        }

        if( client is not null )
        {
            client.ClearPresence();
            client.Dispose();
        }

        Environment.Exit(0);
    }
}
