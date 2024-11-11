using Godot;
using System.Collections.Generic;

public partial class AudioManager : Node
{
    [Export] public int NumPlayers = 8;

    private List<AudioStreamPlayer> available = new List<AudioStreamPlayer>();
    private Queue<AudioOptionsResource> queue = new Queue<AudioOptionsResource>(); // Queue now holds AudioOptionsResource

    private Dictionary<Node, AudioStreamPlayer> nodeToPlayer = new Dictionary<Node, AudioStreamPlayer>();

    public override void _Ready()
    {
        // Create the pool of AudioStreamPlayer nodes
        for (int i = 0; i < NumPlayers; i++)
        {
            var player = new AudioStreamPlayer();
            AddChild(player);
            available.Add(player);
            player.Finished += () => OnStreamFinished(player); // Connect signal
        }
    }

    private void OnStreamFinished(AudioStreamPlayer player)
    {
        available.Add(player); // Make the player available again
    }

    public void Play(AudioOptionsResource options, Node requestingNode)
    {
        if (options != null && options.AudioStream != null)
        {
            queue.Enqueue(options);

            // Track the node and associate the player
            if (!nodeToPlayer.ContainsKey(requestingNode))
            {
                nodeToPlayer.Add(requestingNode, available[0]); // Assign the player to the node
            }
        }
        else
        {
            GD.PrintErr("Invalid AudioOptionsResource provided.");
        }
    }

    public void StopSound(Node requestingNode)
    {
        // Stop the sound and clean up the player reference
        if (nodeToPlayer.ContainsKey(requestingNode))
        {
            var player = nodeToPlayer[requestingNode];
            player.Stop();
            available.Add(player);  // Add the player back to available
            nodeToPlayer.Remove(requestingNode);  // Remove from the dictionary
        }
        else
        {
            GD.PrintErr("No sound playing for the given node.");
        }
    }

    public override void _Process(double delta)
    {
        // Play a queued sound if any players are available
        if (queue.Count > 0 && available.Count > 0)
        {
            var options = queue.Dequeue();
            var player = available[0];
            
            // Set up player properties from AudioOptionsResource
            player.Stream = options.AudioStream;
            player.Bus = options.BusName;
            player.VolumeDb = options.VolumeDb;
            player.PitchScale = options.PitchScale;
            player.Autoplay = false; // Ensure autoplay is off so manual control works
            player.StreamPaused = options.Mute;


            if (options.StartOffset > 0)
            {
                player.Seek(options.StartOffset);
            }

            // Handle 3D sound if specified
            if (options.Is3D)
            {
                var audioPlayer3D = new AudioStreamPlayer3D
                {
                    Stream = options.AudioStream,
                    Position = options.Position,
                    Bus = options.BusName,
                    VolumeDb = options.VolumeDb,
                    PitchScale = options.PitchScale,
                    StreamPaused = options.Mute,
                    Autoplay = false,
                };
                AddChild(audioPlayer3D);
                audioPlayer3D.Play();
            }
            else
            {
                player.Play();
            }

            // Handle fade-in (simplified, can be expanded for smoother interpolation)
            if (options.FadeInDuration > 0)
            {
                // Implement your fade-in logic here (e.g., timer to adjust volume over time)
            }

            available.RemoveAt(0); // Remove player from available list
        }
    }
}
