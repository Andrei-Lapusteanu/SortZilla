using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortZilla
{
    class ZillaConfig
    {

        // Members
        private string folderName;
        private int comboBoxIndex;
        private string comboBoxString;
        private int amount;
        private int amountDummy;

        // Default constructor
        public ZillaConfig()
        {
            folderName = null;
            comboBoxIndex = -1;
            comboBoxString = null;
            amount = -1;
            amountDummy = -1;
        }

        // Custom constructor
        public ZillaConfig(string folderName, int comboBoxIndex, string comboBoxString, int amount, int amountDummy)
        {
            this.folderName = folderName;
            this.comboBoxIndex = comboBoxIndex;
            this.comboBoxString = comboBoxString;
            this.amount = amount;
            this.amountDummy = amountDummy;
        }

        public override string ToString()
        {
            return folderName + '~' + comboBoxIndex + '~' + comboBoxString + '~' + amount + '~' + amountDummy;
        }

        // Properties
        public string FolderName { get => folderName; set => folderName = value; }
        public int ComboBoxIndex { get => comboBoxIndex; set => comboBoxIndex = value; }
        public int Amount { get => amount; set => amount = value; }
        public string ComboBoxString { get => comboBoxString; set => comboBoxString = value; }
        public int AmountDummy { get => amountDummy; set => amountDummy = value; }
    }
}
