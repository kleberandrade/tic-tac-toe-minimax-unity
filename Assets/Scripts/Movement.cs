public class Movement
{
    public int score;
    public Node position;

    public Movement(int score, Node position)
    {
        this.score = score;
        this.position = position;
    }

    public override string ToString()
    {
        return $"[({position.x},{position.y}) = {score}]";
    }
}