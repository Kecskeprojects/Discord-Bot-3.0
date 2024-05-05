namespace Discord_Bot.Communication
{
    public class StaticRoundData(int baseLeftX, int baseRightX, int baseDiagonal, int baseY, int spacing, int totalPositions)
    {
        public int BaseLeftX { get; set; } = baseLeftX;
        public int BaseRightX { get; set; } = baseRightX;
        public int BaseDiagonal { get; set; } = baseDiagonal;
        public int BaseY { get; set; } = baseY;
        public int Spacing { get; set; } = spacing;
        public int TotalPositions { get; set; } = totalPositions;

        public int CalculateY(int positionInRound)
        {
            //If the second half, meaning the right side, is being rendered, and we are not rendering the finalist, restart the multiplier, as we restart from the top
            int multiplier = (TotalPositions != 1 && positionInRound + 1 > (TotalPositions / 2))
                ? (positionInRound - (TotalPositions / 2))
                : positionInRound;
            return BaseY + (multiplier * (Spacing + BaseDiagonal));
        }

        public int CalculateX(int positionInRound)
        {
            //If the second half, meaning the right side, is being rendered, and we are not rendering the finalist, use the larger X value, which is the right one
            return (TotalPositions != 1 && positionInRound + 1 > (TotalPositions / 2))
                ? BaseRightX
                : BaseLeftX;
        }
    }
}
