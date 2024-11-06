using Godot;
using System.Collections.Generic;

public partial class AudioManager : Node
{
    [Export] public int NumPlayers = 8;
    [Export] public string Bus = "Master";

    private List<AudioStreamPlayer> available = new List<AudioStreamPlayer>();
    private Queue<AudioStream> queue = new Queue<AudioStream>(); // Queue now stores AudioStreams


    // Add this dictionary to track sounds by their ID or some reference
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
            player.Bus = Bus;
        }
    }

    private void OnStreamFinished(AudioStreamPlayer player)
    {
        // Make the player available again when finished
        available.Add(player);
    }

    public void Play(string soundPath, Node requestingNode)
    {
        // Load the AudioStream from path and enqueue it
        var stream = GD.Load<AudioStream>(soundPath);
        if (stream != null)
        {
            queue.Enqueue(stream);

            // Track the node that requested the sound
            if (!nodeToPlayer.ContainsKey(requestingNode))
            {
                nodeToPlayer.Add(requestingNode, available[0]); // Assign the player to the node
            }
        }
        else
        {
            GD.PrintErr($"Failed to load audio from path: {soundPath}");
        }
    }

    public void Play(AudioStream audioStream, Node requestingNode)
    {
        // Enqueue the provided AudioStream directly and associate it with a node
        if (audioStream != null)
        {
            queue.Enqueue(audioStream);

            // Track the node that requested the sound
            if (!nodeToPlayer.ContainsKey(requestingNode))
            {
                nodeToPlayer.Add(requestingNode, available[0]); // Assign the player to the node
            }
        }
        else
        {
            GD.PrintErr("Null AudioStream provided.");
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
            var audioStream = queue.Dequeue();
            var player = available[0];
            player.Stream = audioStream;
            player.VolumeDb = -20;
            player.Play();
            available.RemoveAt(0); // Remove player from available list
        }
    }
}