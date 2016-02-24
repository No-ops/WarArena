
using System;
using System.Collections.Generic;
using WarArena.Utilities;

namespace WarArena.World
{
    static class MapCreator
	{
		internal const int MapWidth = 40;
		internal const int  MapHeight = 21;
		const int PercentAreWalls = 40;

		static Tile[,] map = new Tile[MapWidth,MapHeight];

        public static Tile[,] CreateMap()
        {
            Tile[,] map = RandomFillMap();
            map = MakeCaverns(map);
            return map;
        }

        public static Tile[,] CreateEmptyMap()
        {
            Tile[,] map = new Tile[MapWidth,MapHeight];
            for (int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    var isEdge = x == 0 || y == 0 || x == MapWidth - 1 || y == MapHeight - 1;
                    map[x,y] = new Tile(x,y, isEdge);
                }
            }
            return map;
        }

        static Tile[,] MakeCaverns(Tile[,] map)
		{
			for(int row=0; row <= MapHeight-1; row++)
			{
				for(int column = 0; column <= MapWidth-1; column++)
				{
					if (PlaceWallLogic(column,row) == 1)
						map[column,row] = new Tile(column,row,true);
					else
						map[column,row] = new Tile(column,row);
				}
			}

			return map;
		}
		
		static int PlaceWallLogic(int x,int y)
		{

			if ((y == MapHeight / 2 || x == MapWidth / 2)
				&& x>0 && y>0
				&& x<MapWidth-1 && y<MapHeight-1) return 0;
			
			
			int numWalls = GetAdjacentWalls(x,y,1,1);
			
			


			if(map[x,y].IsCaveWall)
			{
				if( numWalls >= 4 )
					return 1;

				if(numWalls<2)
					return 0;
				
			}
			else
			{
				if(numWalls>=5)
					return 1;
			}
			return 0;
		}
		
		static int GetAdjacentWalls(int x,int y,int scopeX,int scopeY)
		{
			int startX = x - scopeX;
			int startY = y - scopeY;
			int endX = x + scopeX;
			int endY = y + scopeY;
			
			int iX = startX;
			int iY = startY;
			
			int wallCounter = 0;
			
			for(iY = startY; iY <= endY; iY++) {
				for(iX = startX; iX <= endX; iX++)
				{
					if(!(iX==x && iY==y))
					{
						if(IsWall(iX,iY))
							wallCounter += 1;
					}
				}
			}
			return wallCounter;
		}
		
		static bool IsWall(int x,int y)
		{
			if( IsOutOfBounds(x,y) )
				return true;
			
			if(map[x,y].IsCaveWall)
				return true;
			
			if(map[x,y].IsCaveWall == false)
				return false;

			return false;
		}
		
		static bool IsOutOfBounds(int x, int y)
		{
			if( x<0 || y<0 )
			{
				return true;
			}
			else if( x>MapWidth - 1|| y>MapHeight - 1)
			{
				return true;
			}
			return false;
		}	
		static Tile[,] RandomFillMap()
		{
			
		
			
			for(int row=0; row < MapHeight; row++) {
				for(int column = 0; column < MapWidth; column++)
				{
					if(column == 0)
						map[column,row] = new Tile(column,row,true);

					else if (row == 0)
						map[column,row] = new Tile(column,row,true);

					else if (column == MapWidth-1) 
						map[column,row] = new Tile(column,row,true);
					
					else if (row == MapHeight-1)
						map[column,row] = new Tile(column,row,true);
					else
					   map[column, row] = new Tile(column, row, RandomizationFunctions.Chance(PercentAreWalls));
						
						

									}
			}
			return map;
		}
	}
}
