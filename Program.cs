using PCDLRN;

namespace 评价数量统计_2020SP03
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //创建PCDIMS实例
            PCDLRN.Application pcApp = new();

            //获取当前运行程序
            PartProgram partProgram = pcApp.ActivePartProgram;
            //获取程序命令
            Commands cmds = partProgram.Commands;

            int count = 0;
            int subCount = 0;
            bool afterPosition = false;
            
            //逐行命令进行筛选识别，注意命令的index从1开始
            for (int i = 1; i <= cmds.Count; i++)
            {
                List<Dictionary<string, string>> datas = new();
                Command cmd = cmds.Item(i);
                //不计算被标记的内容
                if (!cmd.Marked) continue;
                if (cmd.IsDimension)
                {
                    DimensionCmd dim = cmd.DimensionCommand;                    
                    //去除基准数据
                    if (cmd.GetFieldValue[ENUM_FIELD_TYPES.UNIT_TYPE, 0].ToString() != "False")
                    {
                        //识别是否是大小命令 大小(Size)命令中，枚举数的第0位为False，上值在1中，下值在2中，所有实测值0会返回FALSE
                        if (cmd.GetFieldValue[ENUM_FIELD_TYPES.DIM_MEASURED, 0].ToString() == "False")
                        {
                            count++;
                        }
                        //识别出位置的第一行
                        else if (dim.ID.ToString() != " " && cmd.GetFieldValue[ENUM_FIELD_TYPES.AXIS, 0].ToString() == "False")
                        {                         
                            //当再次碰到位置且中间没有TP时，说明时尺寸数值不是位置度，将临时计数计入总数，并将临时计数清零
                            if (afterPosition)
                            {                    
                                count += subCount;
                                subCount = 0;
                            }
                            else
                            {
                                afterPosition = true;                              
                            }
                        }
                        //当在位置后遇到TP时，说明中间的时位置度相关数值，不计入总数仅记录TP数值作为一次计数，将afterPosition变回False
                        else if (dim.AxisLetter.ToString() == "TP" && afterPosition)
                        {
                            
                            count++;
                            subCount = 0;
                            afterPosition = false;
                        }
                        //其余尺寸进行计数，如果在位置后则先进行临时计数
                        else
                        {                    
                            if (afterPosition) subCount++;
                            else count++;
                        }
                    }
            

                }
                else if (cmd.IsFCFCommand)
                {
                    count++;
                }                
            }
            count += subCount;            
            partProgram.StatsCount = count;
            partProgram.RefreshPart();
            MessageBox.Show($"程序名称：{partProgram.PartName}\n评价特征数量：{count}","PC-DIMS评价数量统计");
        }
    }
    
}
