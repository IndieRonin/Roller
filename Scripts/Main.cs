using Godot;
using System;
using System.Collections.Generic;

public class Main : Node2D
{
    //The level that the player is on
    int level = 1;
    //The start position for the ball when instantiated
    Vector2 startPosition;
    //The list of levels "Pre loaded" for use as the game preogresses
    List<PackedScene> levels = new List<PackedScene>();
    //The ball scene that will be instanciated at the start of the level
    PackedScene ballScene;
    //The node for keeping track of the ball and level scene after they have been instanciated
    Node currentLevel, ball;
    //A reference to the hud scene
    CanvasLayer hud;
    //The script reference attached to the hud scene, assinged to a reference as its used many times and I do not want to call getnode every time
    HUD hudScript;
    //A reference to the timer that starts when the level is started
    Timer lapTimer;
    //Stores the minutes for the lap time
    int lapTimeMinutes;
    //Stores the secons for the lap time
    int lapTimeSeconds;
    //The timer used to count down time befoer the level starts
    Timer levelStartTimer;
    //The amount for the count down timer
    int levelStartTime = 3;
    //Ball velocity when pausing the game
    Vector2 ballVelocity;
    //Stores the balls angular velocity when puasung the game
    float ballAngelVelocity;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        lapTimer = GetNode<Timer>("LapTimer");
        levelStartTimer = GetNode<Timer>("CountDownTimer");
        //Get the HUD node to enable the minipulation of its child nodes
        hud = GetNode<CanvasLayer>("HUD");
        //Get the HUD script
        hudScript = GetNode<HUD>("HUD");
        //Load the levels in the list of levels
        levels.Add(ResourceLoader.Load("res://Nodes/Level1.tscn") as PackedScene);

        //Grabs the ball scene and loads it in a container for quick reference
        ballScene = (PackedScene)ResourceLoader.Load("res://Nodes/Ball.tscn");
        //Connect to the HUDs signals for he show menu button functionality
        hud.Connect("ShowMenu", this, nameof(ShowMenu));
        //Connect to the HUDs signals for he show menu button functionality
        hud.Connect("ShowHUD", this, nameof(StartLevel));
    }
    public void StartLevel()
    {
        if (level < levels.Count)
        {
            //Throws error if the amount of levels exceed the list size and returns out of thte method
            GD.PrintErr("Level exceeds the max amount of layers");
            return;
        }
        else
        {
            //Instanciate the level that level is equal to
            currentLevel = levels[(level - 1)].Instance();
            currentLevel.Name = "currentLevel";
        }
        //Instantiate the firts level
        AddChild(currentLevel);
        ball = ballScene.Instance();
        ball.Name = "ball";
        //Inctanciatte the ball
        AddChild(ball);
        //Get the balls start position from the levels SpawnPoint Node2D
        startPosition = GetNode<Node2D>("currentLevel/SpawnPoint").Position;
        //Set hte balls position to the tart points position in the level
        GetNode<RigidBody2D>("ball").Position = startPosition;
        //Removes the balls gravity
        GetNode<RigidBody2D>("ball").GravityScale = 0;
        //Connect to the levels levelcomplete node to connet to the level complete signal
        GetNode<Node2D>("currentLevel/GoalArea").Connect("GoalReached", this, nameof(LapDone));
        //Starts the laps count down timer
        levelStartTimer.Start();
    }
    public void ShowMenu()
    {
        //Removes the balls gravity
        GetNode<RigidBody2D>("ball").GravityScale = 0;
        //Set the velocities of the ball to zero after storing them so tat when the menu is called the ball does not keep moving
        ballVelocity = GetNode<RigidBody2D>("ball").LinearVelocity;
        GetNode<RigidBody2D>("ball").LinearVelocity = Vector2.Zero;
        ballAngelVelocity = GetNode<RigidBody2D>("ball").AngularVelocity;
        GetNode<RigidBody2D>("ball").AngularVelocity = 0f;
        //Stop the timer when the game is paused
        lapTimer.Stop();
    }

    private void LapDone()
    {
        //Save the lap time and reset for next level
        GD.Print("finnished level");
        GetNode<SaveSystem>("/root/SaveSystem");
    }

    public void StoreScore(int score)
    {
        //Read up on how to do it
    }

    public int LoadScore()
    {
        //Read up on how to load the score
        return 0;
    }
    private void LapStartTick()
    {
        hudScript.ShowMessage(levelStartTime.ToString());
        levelStartTime--;

        if (levelStartTime < 0)
        {
            levelStartTime = 3;
            lapTimer.Start();
            levelStartTimer.Stop();
            hudScript.HideMessage();
            GetNode<RigidBody2D>("ball").GravityScale = 5;
        }
    }

    private void LapTimerTick()
    {
        lapTimeSeconds++;
        if (lapTimeSeconds % 60 == 0)
        {
            lapTimeMinutes++;
            lapTimeSeconds = 0;
        }

        hudScript.UpdateLapTime(lapTimeMinutes.ToString() + ":" + lapTimeSeconds.ToString());
    }
}
