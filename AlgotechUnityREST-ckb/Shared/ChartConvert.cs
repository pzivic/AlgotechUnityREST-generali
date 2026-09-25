// Prederag Živić
// 23.07.2010
// konverzija tekstova iz Latinice u ćirilicu (radi konverziju i YUSCII)


namespace AlgotechUnityREST_ckb.Shared
{
    /// <summary>
    /// Konveryija
    /// </summary>
    public static class ChartConvert
    {

        #region Lat2Cyr
        /// <summary>
        /// Konverzija znakova iz latinice u ćirilicu, vidi <see cref="Cyr2Lat"/>.
        /// </summary>
        /// <param name="latin">niz znakova za konverziju.</param>
        /// <returns>
        /// Vraća konvertovani niz znakova.
        /// </returns>
        public static string Lat2Cyr(this string latin)
        {
            var c = "";
            string par;
            int i;


            for (i = 0; i < latin.Length; i++)
            {
                par = latin.Substring(i, i < latin.Length - 1 ? 2 : 1);
                switch (par)
                {
                    case "lj":
                        c += "љ";
                        i++;
                        break; // lj
                    case "Lj":
                        c += "Љ";
                        i++;
                        break;
                    case "LJ":
                        c += "Љ";
                        i++;
                        break;
                    case "lJ":
                        c += "Љ";
                        i++;
                        break;
                    case "nj":
                        c += "њ";
                        i++;
                        break; // nj
                    case "Nj":
                        c += "Њ";
                        i++;
                        break;
                    case "NJ":
                        c += "Њ";
                        i++;
                        break;
                    case "nJ":
                        c += "Њ";
                        i++;
                        break;
                    case "dž":
                        c += "џ";
                        i++;
                        break; // dž
                    case "Dž":
                        c += "Џ";
                        i++;
                        break;
                    case "DŽ":
                        c += "Џ";
                        i++;
                        break;
                    case "dŽ":
                        c += "Џ";
                        i++;
                        break;
                    default:
                        var slovo = par.Substring(0, 1);
                        switch (slovo)
                        {
                            case "a":
                                c += "а";
                                break;
                            case "A":
                                c += "А";
                                break;
                            case "b":
                                c += "б";
                                break;
                            case "B":
                                c += "Б";
                                break;
                            case "c":
                                c += "ц";
                                break;
                            case "C":
                                c += "Ц";
                                break;
                            case "č":
                                c += "ч";
                                break;
                            case "Č":
                                c += "Ч";
                                break;
                            case "ć":
                                c += "ћ";
                                break;
                            case "Ć":
                                c += "Ћ";
                                break;
                            case "d":
                                c += "д";
                                break;
                            case "D":
                                c += "Д";
                                break;
                            case "đ":
                                c += "ђ";
                                break;
                            case "Đ":
                                c += "Ђ";
                                break;
                            case "e":
                                c += "е";
                                break;
                            case "E":
                                c += "Е";
                                break;
                            case "f":
                                c += "ф";
                                break;
                            case "F":
                                c += "Ф";
                                break;
                            case "g":
                                c += "г";
                                break;
                            case "G":
                                c += "Г";
                                break;
                            case "h":
                                c += "х";
                                break;
                            case "H":
                                c += "Х";
                                break;
                            case "i":
                                c += "и";
                                break;
                            case "I":
                                c += "И";
                                break;
                            case "j":
                                c += "ј";
                                break;
                            case "J":
                                c += "Ј";
                                break;
                            case "k":
                                c += "к";
                                break;
                            case "K":
                                c += "К";
                                break;
                            case "l":
                                c += "л";
                                break;
                            case "L":
                                c += "Л";
                                break;
                            case "m":
                                c += "м";
                                break;
                            case "M":
                                c += "М";
                                break;
                            case "n":
                                c += "н";
                                break;
                            case "N":
                                c += "Н";
                                break;
                            case "o":
                                c += "о";
                                break;
                            case "O":
                                c += "О";
                                break;
                            case "p":
                                c += "п";
                                break;
                            case "P":
                                c += "П";
                                break;
                            case "r":
                                c += "р";
                                break;
                            case "R":
                                c += "Р";
                                break;
                            case "s":
                                c += "с";
                                break;
                            case "S":
                                c += "С";
                                break;
                            case "š":
                                c += "ш";
                                break;
                            case "Š":
                                c += "Ш";
                                break;
                            case "t":
                                c += "т";
                                break;
                            case "T":
                                c += "Т";
                                break;
                            case "u":
                                c += "у";
                                break;
                            case "U":
                                c += "У";
                                break;
                            case "v":
                                c += "в";
                                break;
                            case "V":
                                c += "В";
                                break;
                            case "z":
                                c += "з";
                                break;
                            case "Z":
                                c += "З";
                                break;
                            case "ž":
                                c += "ж";
                                break;
                            case "Ž":
                                c += "Ж";
                                break;

                            // pedja yuscii
                            case "`":
                                c += "ж";
                                break;
                            case "~":
                                c += "ч";
                                break;
                            case "{":
                                c += "ш";
                                break;
                            case "}":
                                c += "ћ";
                                break;
                            case "|":
                                c += "ђ";
                                break;
                            case "q":
                                c += "љ";
                                break;
                            case "w":
                                c += "њ";
                                break;
                            case "y":
                                c += "џ";
                                break;
                            case "x":
                                c += "џ";
                                break;

                            // pedja yuscii
                            case "@":
                                c += "Ж";
                                break;
                            case "^":
                                c += "Ч";
                                break;
                            case "[":
                                c += "Ш";
                                break;
                            case "]":
                                c += "Ћ";
                                break;
                            case @"\":
                                c += "Ђ";
                                break;
                            case "Q":
                                c += "Љ";
                                break;
                            case "W":
                                c += "Њ";
                                break;
                            case "Y":
                                c += "Џ";
                                break;
                            case "X":
                                c += "Џ";
                                break;
                            //pedja

                            default:
                                c += slovo;
                                break;
                        }
                        break;
                }
            }
            return c;
        }
        #endregion

        #region Cyr2Lat
        public static string Cyr2Lat(this string latin)
        {
            var c = "";
            string par;
            int i;


            for (i = 0; i < latin.Length; i++)
            {
                par = latin.Substring(i, i < latin.Length - 1 ? 2 : 1);
                switch (par)
                {
                    //case "љ":
                    //    c += "lj";
                    //    i++;
                    //    break; // lj
                    //case "Љ":
                    //    c += "Lj";
                    //    i++;
                    //    break;

                    //case "њ":
                    //    c += "nj";
                    //    i++;
                    //    break; // nj
                    //case "Њ":
                    //    c += "Nj";
                    //    i++;
                    //    break;

                    //case "џ":
                    //    c += "dž";
                    //    i++;
                    //    break; // dž
                    //case "Џ":
                    //    c += "Dž";
                    //    i++;
                    //    break;

                    default:
                        var slovo = par.Substring(0, 1);
                        switch (slovo)
                        {

                            case "љ":
                                c += "lj";
                                break;
                            case "Љ":
                                c += "Lj";
                                break;

                            case "њ":
                                c += "nj";
                                break;
                            case "Њ":
                                c += "Nj";
                                break;

                            case "џ":
                                c += "dž";
                                break;
                            case "Џ":
                                c += "Dž";
                                break;

                            case "а":
                                c += "a";
                                break;
                            case "А":
                                c += "A";
                                break;
                            case "б":
                                c += "b";
                                break;
                            case "Б":
                                c += "B";
                                break;
                            case "ц":
                                c += "c";
                                break;
                            case "Ц":
                                c += "C";
                                break;
                            case "ч":
                                c += "č";
                                break;
                            case "Ч":
                                c += "Č";
                                break;
                            case "ћ":
                                c += "ć";
                                break;
                            case "Ћ":
                                c += "Ć";
                                break;
                            case "д":
                                c += "d";
                                break;
                            case "Д":
                                c += "D";
                                break;
                            case "ђ":
                                c += "đ";
                                break;
                            case "Ђ":
                                c += "Đ";
                                break;
                            case "е":
                                c += "e";
                                break;
                            case "Е":
                                c += "E";
                                break;
                            case "ф":
                                c += "f";
                                break;
                            case "Ф":
                                c += "F";
                                break;
                            case "г":
                                c += "g";
                                break;
                            case "Г":
                                c += "G";
                                break;
                            case "х":
                                c += "h";
                                break;
                            case "Х":
                                c += "H";
                                break;
                            case "и":
                                c += "i";
                                break;
                            case "И":
                                c += "I";
                                break;
                            case "ј":
                                c += "j";
                                break;
                            case "Ј":
                                c += "J";
                                break;
                            case "к":
                                c += "k";
                                break;
                            case "К":
                                c += "K";
                                break;
                            case "л":
                                c += "l";
                                break;
                            case "Л":
                                c += "L";
                                break;
                            case "м":
                                c += "m";
                                break;
                            case "М":
                                c += "M";
                                break;
                            case "н":
                                c += "n";
                                break;
                            case "Н":
                                c += "N";
                                break;
                            case "о":
                                c += "o";
                                break;
                            case "О":
                                c += "O";
                                break;
                            case "п":
                                c += "p";
                                break;
                            case "П":
                                c += "P";
                                break;
                            case "р":
                                c += "r";
                                break;
                            case "Р":
                                c += "R";
                                break;
                            case "с":
                                c += "s";
                                break;
                            case "С":
                                c += "S";
                                break;
                            case "ш":
                                c += "š";
                                break;
                            case "Ш":
                                c += "Š";
                                break;
                            case "т":
                                c += "t";
                                break;
                            case "Т":
                                c += "T";
                                break;
                            case "у":
                                c += "u";
                                break;
                            case "У":
                                c += "U";
                                break;
                            case "в":
                                c += "v";
                                break;
                            case "В":
                                c += "V";
                                break;
                            case "з":
                                c += "z";
                                break;
                            case "З":
                                c += "Z";
                                break;
                            case "ж":
                                c += "ž";
                                break;
                            case "Ж":
                                c += "Ž";
                                break;

                            // pedja yuscii
                            case "`":
                                c += "ž";
                                break;
                            case "~":
                                c += "č";
                                break;
                            case "{":
                                c += "š";
                                break;
                            case "}":
                                c += "ć";
                                break;
                            case "|":
                                c += "đ";
                                break;

                            /*
                            // pedja yuscii
                            case "@":
                                c += "Ž";
                                break;
                            case "^":
                                c += "Č";
                                break;
                            case "[":
                                c += "Š";
                                break;
                            case "]":
                                c += "Ć";
                                break;
                            case @"\":
                                c += "Đ";
                                break;
                            case "Q":
                                c += "Lj";
                                break;
                             * */
                            //case "W":
                            //    c += "nj";
                            //    break;
                            //case "Y":
                            //    c += "Џ";
                            //    break;
                            //case "X":
                            //    c += "Џ";
                            //    break;
                            //pedja

                            default:
                                c += slovo;
                                break;
                        }
                        break;
                }
            }
            return c;
        }
        #endregion

    }
}


