using System;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Poker
{
    public partial class frmPoker : Form
    {
        #region 欄位
        PictureBox[] pic = new PictureBox[5];
        int[] allPoker = new int[52];
        int[] playerPoker = new int[5];
        int totalMoney = 1000000;
        int betAmount = 0;
        #endregion

        public frmPoker()
        {
            InitializeComponent();
            InitializePoker();
        }

        #region 自定義方法
        private void InitializePoker()
        {
            for (int i = 0; i < pic.Length; i++)
            {
                pic[i] = new PictureBox();
                pic[i].Image = GetImage("back");
                pic[i].Name = "pic" + i;
                pic[i].SizeMode = PictureBoxSizeMode.AutoSize;
                pic[i].Top = 30;
                pic[i].Left = 10 + ((pic[i].Width + 10) * i);
                pic[i].Enabled = false;
                pic[i].Tag = "back";
                pic[i].Visible = true;
                this.grpPoker.Controls.Add(pic[i]);
                pic[i].Click += Pic_Click;
            }
        }

        private void ShowCards()
        {
            for (int i = 0; i < playerPoker.Length; i++)
                pic[i].Image = GetImage("pic" + (playerPoker[i] + 1));
        }

        private Image GetImage(string name)
        {
            return Properties.Resources.ResourceManager.GetObject(name) as Image;
        }

        private Image GetImage(int num)
        {
            return GetImage("pic" + num);
        }

        private void Shuffle()
        {
            Random rand = new Random();
            for (int i = 0; i < 1000; i++)
            {
                int r = rand.Next(allPoker.Length);
                int temp = allPoker[r];
                allPoker[r] = allPoker[0];
                allPoker[0] = temp;
            }
        }

        private int GetOdds(string result)
        {
            if (result.Contains("同花大順")) return 250;
            if (result.Contains("同花順"))   return 50;
            if (result.Contains("鐵支"))     return 25;
            if (result.Contains("葫蘆"))     return 9;
            if (result.Contains("同花"))     return 6;
            if (result.Contains("順子"))     return 4;
            if (result.Contains("三條"))     return 3;
            if (result.Contains("兩對"))     return 2;
            if (result.Contains("一對"))     return 1;
            return 0;
        }
        #endregion

        #region 事件處理程序
        private void Pic_Click(object sender, EventArgs e)
        {
            PictureBox p = sender as PictureBox;
            int index = int.Parse(p.Name.Replace("pic", ""));
            int cardNum = playerPoker[index] + 1;
            if (p.Tag.ToString() == "back")
            {
                p.Tag = "front";
                p.Image = GetImage(cardNum);
            }
            else
            {
                p.Tag = "back";
                p.Image = GetImage("back");
            }
        }

        private void btnBet_Click(object sender, EventArgs e)
        {
            int input;
            if (!int.TryParse(txtBetAmount.Text.Trim(), out input) || input <= 0)
            {
                MessageBox.Show("請輸入正確的押注金額（正整數）！", "輸入錯誤",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (input > totalMoney)
            {
                MessageBox.Show("押注金額不可超過總資金 " + totalMoney.ToString("N0") + " 元！", "資金不足",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            betAmount = input;
            totalMoney -= betAmount;
            txtTotalMoney.Text = totalMoney.ToString();
            btnBet.Enabled = false;
            txtBetAmount.Enabled = false;
            btnDealCard.Enabled = true;
        }

        private async void btnDealCard_Click(object sender, EventArgs e)
        {
            lblResult.Text = "";
            for (int i = 0; i < pic.Length; i++)
                pic[i].Image = GetImage("back");
            for (int i = 0; i < allPoker.Length; i++)
                allPoker[i] = i;
            Shuffle();
            await Task.Delay(500);
            for (int i = 0; i < playerPoker.Length; i++)
                playerPoker[i] = allPoker[i];
            ShowCards();
            for (int i = 0; i < pic.Length; i++)
            {
                pic[i].Enabled = true;
                pic[i].Tag = "front";
            }
            btnChangeCard.Enabled = true;
            btnDealCard.Enabled = false;
        }

        private void btnChangeCard_Click(object sender, EventArgs e)
        {
            int startIndex = 5;
            for (int i = 0; i < playerPoker.Length; i++)
            {
                if (pic[i].Tag.ToString() == "back")
                {
                    playerPoker[i] = allPoker[startIndex];
                    pic[i].Image = GetImage(playerPoker[i] + 1);
                    pic[i].Tag = "front";
                    startIndex++;
                }
            }
            for (int i = 0; i < pic.Length; i++)
                pic[i].Enabled = false;
            btnChangeCard.Enabled = false;
            btnCheck.Enabled = true;
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            string[] colorList = { "梅花", "方塊", "愛心", "黑桃" };
            string[] pointList = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
            int[] pokerColor = new int[5];
            int[] pokerPoint = new int[5];
            for (int i = 0; i < playerPoker.Length; i++)
            {
                pokerColor[i] = playerPoker[i] % 4;
                pokerPoint[i] = playerPoker[i] / 4;
            }
            int[] colorCount = new int[4];
            int[] pointCount = new int[13];
            for (int i = 0; i < pokerColor.Length; i++)
            {
                colorCount[pokerColor[i]]++;
                pointCount[pokerPoint[i]]++;
            }
            Array.Sort(colorCount, colorList);
            Array.Reverse(colorCount);
            Array.Reverse(colorList);
            Array.Sort(pointCount, pointList);
            Array.Reverse(pointCount);
            Array.Reverse(pointList);

            bool isFlush         = (colorCount[0] == 5);
            bool isSingle        = (pointCount[0] == 1 && pointCount[1] == 1 && pointCount[2] == 1 && pointCount[3] == 1 && pointCount[4] == 1);
            bool isDiffFour      = (pokerPoint.Max() - pokerPoint.Min() == 4);
            bool isRoyal         = pokerPoint.Contains(0) && pokerPoint.Contains(9) && pokerPoint.Contains(10) && pokerPoint.Contains(11) && pokerPoint.Contains(12);
            bool isRoyalFlush    = isFlush && isRoyal;
            bool isStraightFlush = isFlush && isSingle && isDiffFour;
            bool isStraight      = isSingle && (isDiffFour || isRoyal);
            bool isFourOfAKind   = (pointCount[0] == 4);
            bool isFullHouse     = (pointCount[0] == 3 && pointCount[1] == 2);
            bool isThreeOfAKind  = (pointCount[0] == 3 && pointCount[1] == 1);
            bool isTwoPair       = (pointCount[0] == 2 && pointCount[1] == 2);
            bool isOnePair       = (pointCount[0] == 2 && pointCount[1] == 1);

            string result;
            if (isRoyalFlush)         result = colorList[0] + " 同花大順";
            else if (isStraightFlush) result = colorList[0] + " 同花順";
            else if (isStraight)      result = "順子";
            else if (isFourOfAKind)   result = pointList[0] + " 鐵支";
            else if (isFullHouse)     result = pointList[0] + "三張" + pointList[1] + "兩張 葫蘆";
            else if (isFlush)         result = colorList[0] + " 同花";
            else if (isThreeOfAKind)  result = pointList[0] + " 三條";
            else if (isTwoPair)       result = pointList[0] + "," + pointList[1] + " 兩對";
            else if (isOnePair)       result = pointList[0] + " 一對";
            else                      result = "雜牌";

            lblResult.Text = result;

            int odds = GetOdds(result);
            int winAmount = betAmount * odds;
            totalMoney += winAmount;
            txtTotalMoney.Text = totalMoney.ToString();

            if (winAmount > 0)
                MessageBox.Show("牌型：" + result + "\n賠率：" + odds + " 倍\n中獎金額：" + winAmount.ToString("N0") + " 元\n總資金：" + totalMoney.ToString("N0") + " 元",
                    "恭喜獲獎！", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("牌型：" + result + "\n未中獎，損失 " + betAmount.ToString("N0") + " 元\n總資金：" + totalMoney.ToString("N0") + " 元",
                    "未中獎", MessageBoxButtons.OK, MessageBoxIcon.Information);

            btnChangeCard.Enabled = false;
            btnCheck.Enabled = false;

            if (totalMoney > 0)
            {
                btnBet.Enabled = true;
                txtBetAmount.Enabled = true;
                btnDealCard.Enabled = false;
            }
            else
            {
                MessageBox.Show("資金已耗盡，遊戲結束！", "遊戲結束",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void frmPoker_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (btnDealCard.Enabled == false && btnChangeCard.Enabled == true)
            {
                switch (e.KeyChar)
                {
                    case 'q':
                        playerPoker[0] = 51; playerPoker[1] = 47;
                        playerPoker[2] = 43; playerPoker[3] = 39; playerPoker[4] = 3;
                        break;
                    case 'w':
                        playerPoker[0] = 37; playerPoker[1] = 33;
                        playerPoker[2] = 29; playerPoker[3] = 25; playerPoker[4] = 21;
                        break;
                    case 'e':
                        playerPoker[0] = 50; playerPoker[1] = 38;
                        playerPoker[2] = 34; playerPoker[3] = 22; playerPoker[4] = 18;
                        break;
                    case 'r':
                        playerPoker[0] = 48; playerPoker[1] = 39;
                        playerPoker[2] = 38; playerPoker[3] = 37; playerPoker[4] = 36;
                        break;
                    case 't':
                        playerPoker[0] = 30; playerPoker[1] = 29;
                        playerPoker[2] = 6;  playerPoker[3] = 5;  playerPoker[4] = 4;
                        break;
                    case 'y':
                        playerPoker[0] = 48; playerPoker[1] = 39;
                        playerPoker[2] = 15; playerPoker[3] = 14; playerPoker[4] = 13;
                        break;
                }
                ShowCards();
            }
        }
        #endregion
    }
}
