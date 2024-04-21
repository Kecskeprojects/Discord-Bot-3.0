namespace Discord_Bot.Communication
{
    public class StaticRoundData
    {
        public StaticRoundData(int baseLeftX, int baseRightX, int baseDiagonal, int baseY, int spacing)
        {
            BaseLeftX = baseLeftX;
            BaseRightX = baseRightX;
            BaseDiagonal = baseDiagonal;
            BaseY = baseY;
            Spacing = spacing;
        }

        public int BaseLeftX { get; set; }
        public int BaseRightX { get; set; }
        public int BaseDiagonal { get; set; }
        public int BaseY { get; set; }
        public int Spacing { get; set; }

        public int CalculateY(int roundStep)
        {
            return BaseY + ((roundStep + 1) * (Spacing + BaseDiagonal));
        }
        public int CalculateX(int roundStep, int roundTotalStep)
        {
            return ((roundStep + 1) > (roundTotalStep / 2)) ? BaseLeftX : BaseRightX;
        }
    }
}
