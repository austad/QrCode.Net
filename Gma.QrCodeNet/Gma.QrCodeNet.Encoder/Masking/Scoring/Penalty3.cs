﻿namespace Gma.QrCodeNet.Encoding.Masking.Scoring
{
	internal class Penalty3 : Penalty
    {

        private Penalty3CheckTree bitCheckTree = new Penalty3CheckTree();

        internal override int PenaltyCalculate(BitMatrix matrix)
        {
            Size size = matrix.Size;
            int penaltyValue = 0;
            for (int i = 0; i < size.Height; i++)
            {
                penaltyValue += MidPatternSearch(matrix, new Point(0, i), true);
            }

            for (int i = 0; i < size.Width; i++)
            {
                penaltyValue += MidPatternSearch(matrix, new Point(i, 0), false);
            }


            return penaltyValue;
        }

        private int MidPatternSearch(BitMatrix matrix, Point position, bool isHorizontal)
        {
            return MidPatternSearch(matrix, position, 0, isHorizontal);
        }


        private int MidPatternSearch(BitMatrix matrix, Point position, int indexJumpValue, bool isHorizontal)
        {
            Size size = matrix.Size;
            Point newPosition;
            if (isInsideMatrix(size, position, indexJumpValue, isHorizontal))
                newPosition = isHorizontal ? position.Offset(indexJumpValue, 0)
                    : position.Offset(0, indexJumpValue);
            else
                return 0;
            return MidPatternCheck(matrix, newPosition, bitCheckTree.Root, isHorizontal);
        }


        private int MidPatternCheck(BitMatrix matrix, Point position, BitBinaryTreeNode<Penalty3NodeValue> checkNode, bool isHorizontal)
        {
            Penalty3NodeValue checkValue = checkNode.Value;
            Size size = matrix.Size;
            if( checkValue.IndexJumpValue > 0 )
            {
                return MidPatternSearch(matrix, position, checkValue.IndexJumpValue, isHorizontal);
            }
            else if (checkValue.IndexJumpValue == 0)
            {
                int penaltyValue = PatternCheck(matrix, position, isHorizontal);
                return penaltyValue + MidPatternSearch(matrix, position, 4, isHorizontal);
            }
            else
            {
                Point checkIndex;
                if (isInsideMatrix(size, position, checkValue.BitCheckIndex, isHorizontal))
                    checkIndex = isHorizontal ? position.Offset(checkValue.BitCheckIndex, 0)
                    : position.Offset(0, checkValue.BitCheckIndex);
                else
                    return 0;

                return matrix[checkIndex] ? MidPatternCheck(matrix, position, checkNode.One, isHorizontal)
                    : MidPatternCheck(matrix, position, checkNode.Zero, isHorizontal);
                
            }
        }


        private int PatternCheck(BitMatrix matrix, Point position, bool isHorizontal)
        {
            Point FrontOnePos;
            FrontOnePos = isHorizontal ? position.Offset(-1, 0)
                : position.Offset(0, -1);
            Point EndOnePos;
            EndOnePos = isHorizontal ? position.Offset(5, 0)
                : position.Offset(0, 5);
            Size size = matrix.Size;
            if (isOutsideMatrix(size, EndOnePos))
            {
                return 0;
            }
            else if (isOutsideMatrix(size, FrontOnePos))
            {
                return MidPatternSearch(matrix, position, 4, isHorizontal);
            }
            else
            {
                if (matrix[FrontOnePos] && matrix[EndOnePos])
                    return LightAreaCheck(matrix, position, isHorizontal);
                else
                    return 0;
            }

        }

        private int LightAreaCheck(BitMatrix matrix, Point position, bool isHorizontal)
        {
            Size size = matrix.Size;
            Point LeftCheckPoint = isHorizontal ? position.Offset(-5, 0)
                : position.Offset(0, -5);
            Point RightCheckPoint = isHorizontal ? position.Offset(9, 0)
                : position.Offset(0, 9);
            int penaltyValue = 0;
            int WhiteModuleCount = 0;

            if(isInsideMatrix(size, LeftCheckPoint))
            {
                for (int i = 0; i < 4; i++)
                {
                    if (matrix[LeftCheckPoint] == false)
                    {
                        WhiteModuleCount++;
                        LeftCheckPoint = isHorizontal ? LeftCheckPoint.Offset(1, 0)
                            : LeftCheckPoint.Offset(0, 1);
                    }
                    else
                        break;
                }

                penaltyValue = WhiteModuleCount == 4 ? 40 + penaltyValue
                    : penaltyValue;
            }

            WhiteModuleCount = 0;

            if (isInsideMatrix(size, RightCheckPoint))
            {
                for (int i = 0; i < 4; i++)
                {
                    if (matrix[RightCheckPoint] == false)
                    {
                        WhiteModuleCount++;
                        RightCheckPoint = isHorizontal ? RightCheckPoint.Offset(-1, 0)
                            : RightCheckPoint.Offset(0, -1);
                    }
                    else
                        break;
                }
                penaltyValue = WhiteModuleCount == 4 ? 40 + penaltyValue
                    : penaltyValue;
            }


            return penaltyValue;
        }


        private bool isOutsideMatrix(Size size, Point position)
        {
            return position.X >= size.Width || position.X < 0 || position.Y >= size.Height || position.Y < 0;
        }

        private bool isInsideMatrix(Size size, Point position)
        {
            return !isOutsideMatrix(size, position);
        }

        private bool isInsideMatrix(Size size, Point position, int indexJumpValue, bool isHorizontal)
        {
            if (isHorizontal)
            {
                return size.Width > (position.X + indexJumpValue);
            }
            else
            {
                return size.Height > (position.Y + indexJumpValue);
            }
        }

    }
}
