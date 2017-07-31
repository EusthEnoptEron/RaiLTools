using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaiLTools.RScript
{

    public class CommandToken
    {
        private List<int> _Params = new List<int>();
        private List<Type> _Types = new List<Type>();
        public int Index { get; private set; }
        public int Value { get; private set; }

        public CommandToken(int index, int value)
        {
            Index = index;
            Value = value;
        }

        public void AddParam(short value)
        {
            _Types.Add(typeof(short));
            _Params.Add(value);
        }

        public void AddParam(int value)
        {
            _Types.Add(typeof(int));
            _Params.Add(value);
        }

        public int[] Params
        {
            get
            {
                return _Params.ToArray();
            }
        }
    }

    /// <summary>
    /// Reverse engineered from 紅殻町博物誌
    /// </summary>
    public class CommandTokenizer
    {
        byte[] _Array;
        int _Pointer = 0;
        public CommandTokenizer(byte[] byteArray)
        {
            _Array = byteArray;
        }

        private int ReadInt32(int index)
        {
            _Pointer += 4;

            byte[] buffer = new byte[4];
            Array.Copy(_Array, index, buffer, 0, 4);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);

            }
            int result = BitConverter.ToInt32(buffer, 0);
            return result;
        }

        private short ReadInt16(int index)
        {
            _Pointer += 2;

            byte[] buffer = new byte[2];
            Array.Copy(_Array, index, buffer, 0, 2);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);

            }
            return BitConverter.ToInt16(buffer, 0);
        }


        private short ReadInt16AsInt32(int index)
        {
            return ReadInt16(index);
        }

        private int ReadInt32_takeLower16()
        {
            int value = ReadInt16(_Pointer);
            ReadInt16(_Pointer);
            return value;
        }


        public IEnumerable<CommandToken> Enumerate()
        {
            while (_Pointer <= _Array.Length - 2)
            {
                int int16_controlNo = ReadInt16(_Pointer);
                CommandToken token = new CommandToken(_Pointer - 2, int16_controlNo);
                int result;
                // if ( !isImage )
                //   setWhenNotAnImage = valueSetWhenNotAnImage;
                // if ( dword_4821B8 )
                // {
                //   dword_4821C0 = 1;
                //   dword_4821B8 = 0;
                // }
                if ((int16_controlNo & 0xF000) > 0)
                {
                    int v1 = (int16_controlNo & 0xF000);
                    if (v1 > 0x8000)
                    {
                        if (v1 > 49152)
                        {
                            switch (v1)
                            {
                                case 53248:
                                    token.AddParam(ReadInt16(_Pointer));
                                    token.AddParam(ReadInt16AsInt32(_Pointer));
                                    token.AddParam(ReadInt16AsInt32(_Pointer));
                                    // sub_41BBD0(int16_controlNo, v43, v44, v45);
                                    break;
                                case 57344:
                                    token.AddParam(ReadInt16(_Pointer));
                                    token.AddParam(ReadInt16AsInt32(_Pointer));
                                    token.AddParam(ReadInt16AsInt32(_Pointer));
                                    // sub_41BC10(int16_controlNo, v40, v41, v42);
                                    break;
                                case 61440:
                                    token.AddParam(ReadInt16(_Pointer));
                                    token.AddParam(ReadInt16AsInt32(_Pointer));

                                    // sub_41B870(int16_controlNo, v38, v39);
                                    break;
                            }
                        }
                        else
                        {
                            switch (v1)
                            {
                                case 49152:
                                    token.AddParam(ReadInt16(_Pointer));
                                    token.AddParam(ReadInt16AsInt32(_Pointer));
                                    token.AddParam(ReadInt16AsInt32(_Pointer));

                                    // sub_41BB90(int16_controlNo, v35, v36, v37);
                                    break;
                                case 36864:
                                    token.AddParam(ReadInt16(_Pointer));
                                    token.AddParam(ReadInt16AsInt32(_Pointer));
                                    token.AddParam(ReadInt16AsInt32(_Pointer));

                                    // sub_41BAD0(int16_controlNo, v32, v33, v34);
                                    break;
                                case 40960:
                                    token.AddParam(ReadInt16(_Pointer));
                                    token.AddParam(ReadInt16AsInt32(_Pointer));
                                    token.AddParam(ReadInt16AsInt32(_Pointer));

                                    // sub_41BB10(int16_controlNo, v29, v30, v31);
                                    break;
                                case 45056:
                                    token.AddParam(ReadInt16(_Pointer));
                                    token.AddParam(ReadInt16AsInt32(_Pointer));
                                    token.AddParam(ReadInt16AsInt32(_Pointer));

                                    // sub_41BB50(int16_controlNo, v26, v27, v28);
                                    break;
                            }
                        }
                    }
                    else if (v1 == 0x8000)
                    {
                        token.AddParam(ReadInt16(_Pointer));
                        token.AddParam(ReadInt16AsInt32(_Pointer));
                        token.AddParam(ReadInt16AsInt32(_Pointer));


                        // sub_41BA90(int16_controlNo, v23, v24, v25);
                    }
                    else if (v1 > 0x4000)
                    {
                        switch (v1)
                        {
                            case 20480:
                                token.AddParam(ReadInt16(_Pointer));
                                token.AddParam(ReadInt16AsInt32(_Pointer));
                                token.AddParam(ReadInt16AsInt32(_Pointer));

                                // sub_41B9D0(int16_controlNo, v20, v21, v22);
                                break;
                            case 24576:
                                token.AddParam(ReadInt16(_Pointer));
                                token.AddParam(ReadInt16AsInt32(_Pointer));
                                token.AddParam(ReadInt16AsInt32(_Pointer));
                                // sub_41BA10(int16_controlNo, v17, v18, v19);


                                break;
                            case 28672:
                                token.AddParam(ReadInt16(_Pointer));
                                token.AddParam(ReadInt16AsInt32(_Pointer));
                                token.AddParam(ReadInt16AsInt32(_Pointer));
                                // sub_41BA50(int16_controlNo, v14, v15, v16);

                                break;
                        }
                    }
                    else
                    {
                        switch (v1)
                        {
                            case 0x4000:
                                token.AddParam(ReadInt16(_Pointer));
                                token.AddParam(ReadInt16AsInt32(_Pointer));
                                token.AddParam(ReadInt16AsInt32(_Pointer));


                                // sub_41B990(int16_controlNo, v11, v12, v13);
                                break;
                            case 0x1000:
                                token.AddParam(ReadInt16(_Pointer));
                                token.AddParam(ReadInt16AsInt32(_Pointer));
                                token.AddParam(ReadInt16AsInt32(_Pointer));
                                // sub_41B8A0(int16_controlNo, v8, v9, v10);
                                break;
                            case 0x2000:
                                token.AddParam(ReadInt16(_Pointer));
                                token.AddParam(ReadInt16AsInt32(_Pointer));
                                token.AddParam(ReadInt16AsInt32(_Pointer));
                                // sub_41B8D0(int16_controlNo, v5, v6, v7);
                                break;
                            case 0x3000:
                                token.AddParam(ReadInt16(_Pointer));
                                token.AddParam(ReadInt16AsInt32(_Pointer));
                                token.AddParam(ReadInt16AsInt32(_Pointer));
                                // sub_41B930(int16_controlNo, v2, v3, v4);
                                break;
                        }
                    }
                }
                else
                {
                    result = 0;
                    switch (int16_controlNo)
                    {
                        case 3:
                            token.AddParam(ReadInt32(_Pointer));
                            // sub_41BC90(v47);

                            break;
                        case 4:
                            token.AddParam(ReadInt32(_Pointer));
                            // sub_41BC70(v48);
                            break;
                        case 5:
                            token.AddParam(ReadInt32(_Pointer));
                            // sub_41BCB0(v49);
                            break;
                        case 9:
                            token.AddParam(ReadInt16(_Pointer));
                            // sub_41BC50(v50);
                            break;
                        case 0xA:

                            // sub_4207F0();
                            break;
                        case 0xB:

                            // sub_4208E0();
                            break;
                        case 0xC:
                            token.AddParam(ReadInt32_takeLower16());
                            //// token.AddParam(v51)
                            token.AddParam(ReadInt32(_Pointer));
                            //result = // sub_41D990(v52, v53);
                            //if (result)
                            break;
                        //return result;
                        case 0xD:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_420780(v54);
                            break;
                        case 0xE:
                            int word_49DEB8 = ReadInt16(_Pointer);
                            token.AddParam(ReadInt32(_Pointer));
                            //v56 = // sub_413AC0(v55);
                            // sub_41D7E0(&v484, v56);
                            int dword_49DED4 = ReadInt32(_Pointer);
                            int dword_49DED8 = ReadInt32(_Pointer);
                            int dword_49DEDC = ReadInt32(_Pointer);
                            int dword_49DEE0 = ReadInt32(_Pointer);
                            int dword_49DEE4 = ReadInt32(_Pointer);
                            token.AddParam(ReadInt32(_Pointer));
                            //v58 = // sub_413AC0(v57);
                            // sub_41D7E0(&v485, v58);
                            token.AddParam(ReadInt32(_Pointer));
                            //v60 = // sub_413AC0(v59);
                            // sub_41D7E0(&v486, v60);
                            token.AddParam(ReadInt32(_Pointer));
                            //v62 = // sub_413AC0(v61);
                            // sub_41D7E0(&v487, v62);
                            token.AddParam(ReadInt32(_Pointer));
                            //v64 = // sub_413AC0(v63);
                            // sub_41D7E0(&v488, v64);
                            token.AddParam(ReadInt32(_Pointer));
                            //v66 = // sub_413AC0(v65);
                            // sub_41D7E0(&v489, v66);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v67);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v69);
                            token.AddParam(ReadInt32_takeLower16());
                            //dword_49DEBC = (int)&v484;
                            //dword_49DEC0 = (int)&v485;
                            //dword_49DEC4 = (int)&v486;
                            //dword_49DEC8 = (int)&v487;
                            //dword_49DECC = (int)&v488;
                            //dword_49DED0 = (int)&v489;
                            // sub_4209F0(&word_49DEB8, v68, v70, v71);
                            break;
                        case 0xF:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v72);
                            token.AddParam(ReadInt32(_Pointer));
                            //v75 = &v483;
                            int v76 = 10;
                            do
                            {
                                token.AddParam(ReadInt32_takeLower16());
                                //v75 += 2;
                                --v76;
                            }
                            while (v76 > 0);
                            //dword_482A98[(unsigned __int16)dword_482AC0] = // sub_413B40(currentPointer);
                            //result = // sub_41DA10(v73, v74, &v483);
                            //if (result)
                            break;
                        //return result;
                        case 0xC8:
                            token.AddParam(ReadInt32(_Pointer));
                            //v78 = &v483;
                            int v79 = 10;
                            do
                            {
                                token.AddParam(ReadInt32_takeLower16());
                                //v78 += 2;
                                --v79;
                            }
                            while (v79 > 0);
                            //dword_482A98[(unsigned __int16)dword_482AC0] = // sub_413B40(currentPointer);
                            //result = // sub_41DAF0(v77, &v483);
                            //if (result)
                            break;
                        //return result;
                        case 0x10:
                            token.AddParam(ReadInt32_takeLower16());
                            //result = // sub_41DB80(v80);
                            //if (result)
                            break;
                        //return result;
                        case 0x11:

                            // sub_421950();
                            break;
                        case 0x12:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v81);
                            token.AddParam(ReadInt32(_Pointer));
                            // sub_421430(v83, v82);
                            break;
                        case 0xCA:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v84);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v86);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_421460(v85, v87, v88);
                            break;
                        case 0x13:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_421A30(v89);
                            break;
                        case 0x3C:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v90);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v92);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_421870(v91, v93, v94);
                            break;
                        case 0x3D:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v95);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_4218C0(v96, v97);
                            break;
                        case 0x3E:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_4216C0(v98);
                            break;
                        case 0x3F:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v99);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v101);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_421730(v100, v102, v103);
                            break;
                        case 0x40:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_4217C0(v104);
                            break;
                        case 0x41:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_421900(v105);
                            break;
                        case 0x42:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v106);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v108);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v110);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_4217F0(v107, v109, v111, v112);
                            break;
                        case 0x43:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_421850(v113);
                            break;
                        case 0x44:

                            // sub_4219B0();
                            break;
                        case 0x45:

                            // sub_4219F0();
                            break;
                        case 0x14:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v114);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F250(v115, v116);
                            break;
                        case 0x15:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F2E0(v117);
                            break;
                        case 0x16:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v118);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v120);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v122);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F330(v119, v121, v123, v124);
                            break;
                        case 0xE6:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F360(v125);
                            break;
                        case 0x17:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v126);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v128);
                            ReadInt32_takeLower16();
                            ReadInt32_takeLower16();
                            // sub_4215D0(v127, v129);
                            break;
                        case 0x18:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v130);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_421640(v131, v132);
                            break;
                        case 0x1E:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v133);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v135);
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v139);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F3D0(v134, v136, v140, v137, v138, v141);
                            break;
                        case 0x3B:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v142);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v144);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v146);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F420(v143, v145, v147, v148);
                            break;
                        case 0x20:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v149);
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v153);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v155);
                            token.AddParam(ReadInt32(_Pointer));
                            // sub_41F480(v150, v157, v154, v151, v152, v156);
                            break;
                        case 0x21:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v158);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v160);
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v163);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F4D0(v159, v164, v161, v162, v165);
                            break;
                        case 0x22:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v166);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v168);
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v171);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F590(v167, v172, v169, v170, v173);
                            break;
                        case 0x23:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v174);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F650(v175, v176);
                            break;
                        case 0x24:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v177);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F6E0(v178, v179);
                            break;
                        case 0x25:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v180);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F770(v181, v182);
                            break;
                        case 0x26:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v183);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v185);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v187);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F890(v184, v186, v188, v189);
                            break;
                        case 0x27:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v190);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v192);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F7F0(v191, v193, v194);
                            break;
                        case 0x28:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v195);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F940(v196, v197);
                            break;
                        case 0x29:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v198);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F9E0(v199, v200);
                            break;
                        case 0xC9:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v201);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v203);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v205);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v207);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41FA80(v202, v204, v206, v208, v209);
                            break;
                        case 0xE7:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F9C0(v210);
                            break;
                        case 0x2A:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v211);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F220(v212, v213);
                            break;
                        case 0x2B:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v214);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41FC30(v215, v216);
                            break;
                        case 0x2C:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41FDA0(v217);
                            break;
                        case 0x2D:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v218);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41FDF0(v219, v220);
                            break;
                        case 0x2E:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41FEC0(v221);
                            break;
                        case 0x2F:
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EC40(v222, v223);
                            break;
                        case 0x30:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v224);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v226);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F450(v225, v227, v228);
                            break;
                        case 0x31:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v229);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41FA00(v230, v231);
                            break;
                        case 0x50:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EE80(v232);
                            break;
                        case 0x51:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v233);
                            token.AddParam(ReadInt32(_Pointer));
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v236);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v238);
                            ReadInt32(_Pointer);
                            token.AddParam(ReadInt32(_Pointer));
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41F020(v234, v240, (unsigned __int16)v241, v235, v237, v239);
                            break;
                        case 0x52:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v242);
                            ReadInt32_takeLower16();
                            ReadInt32_takeLower16();
                            ReadInt32_takeLower16();
                            token.AddParam(ReadInt32(_Pointer));
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EEB0(v243, v244, (unsigned __int16)v245);
                            break;
                        case 0x53:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41ECD0(v246);
                            break;
                        case 0x32:

                            // sub_4206F0();
                            break;
                        case 0x33:

                            // sub_420700();
                            break;
                        case 0x34:

                            // sub_4207C0();
                            break;
                        case 0x35:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_4206C0(v247);
                            break;
                        case 0x37:

                            // sub_420770();
                            break;
                        case 0x38:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v248);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v250);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v252);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v254);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_420670(v249, v251, v253, v255, v256);
                            break;
                        case 0x39:

                            // sub_4206E0();
                            break;
                        case 0x1A:

                            //__setargv();
                            break;
                        case 0x1B:

                            // sub_41FF20();
                            break;
                        case 0x1C:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v257);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v259);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41FF40(v258, v260, v261);
                            break;
                        case 0x1D:
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41FF70(v262, v263);
                            break;
                        case 0x3A:

                            //if (!word_4821F8)
                            break;
                        //return result;
                        case 0x19:
                            token.AddParam(ReadInt32(_Pointer));
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_420E60(v264, v265);
                            break;
                        case 0x46:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v266);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v268);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v270);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41FFA0(v267, v269, v271, v272);
                            break;
                        case 0x47:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v273);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v275);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v277);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41FFD0(v274, v276, v278, v279);
                            break;
                        case 0x48:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_420030(v280);
                            break;
                        case 0x49:
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_420080(v281, v282, v283);
                            break;
                        case 0x4A:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_420250(v284);
                            break;
                        case 0x4B:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v285);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v287);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v289);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v291);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_420050(v286, v288, v290, v292, v293);
                            break;
                        case 0x4D:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v294);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v296);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v298);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_420000(v295, v297, v299, v300);
                            break;
                        case 0x5A:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v301);
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EB90(v302, v303, v304);
                            break;
                        case 0x5B:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v305);
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v309);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EB40(v306, v307, v308, v310, v311);
                            break;
                        case 0x5C:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v312);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EA90(v313, v314);
                            break;
                        case 0x5D:
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EC20(v315, v316);
                            break;
                        case 0x5E:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EBC0(v317);
                            break;
                        case 0x5F:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v318);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41E900(v319, v320);
                            break;
                        case 0x60:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v321);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EA10(v322, v323);
                            break;
                        case 0x61:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v324);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EA30(v325, v326);
                            break;
                        case 0x62:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v327);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EA50(v328, v329);
                            break;
                        case 0x63:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v330);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v332);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EA70(v331, v333, v334);
                            break;
                        case 0x64:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v335);
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EAB0(v336, v337, v338);
                            break;
                        case 0x65:
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EBE0(v339, v340);
                            break;
                        case 0x69:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v341);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EC00(v342, v343);
                            break;
                        case 0x66:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_4206D0(v344);
                            break;
                        case 0x67:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v345);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EAE0(v346, v347);
                            break;
                        case 0x68:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v348);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v350);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v352);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EB00(v349, v351, v353, v354);
                            break;
                        case 0x6E:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v355);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v357);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_4214B0(v356, v358, v359);
                            break;
                        case 0x6F:
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_421510(v360, v361, v362);
                            break;
                        case 0x70:
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_421530(v363);
                            break;
                        case 0x71:
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_421560(v364, v365);
                            break;
                        case 0x72:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v366);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_421590(v367, v368);
                            break;
                        case 0x78:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v369);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41FB00(v370, v371);
                            break;
                        case 0x79:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v372);
                            token.AddParam(ReadInt32(_Pointer));
                            // sub_41FB80(v373, v374);
                            break;
                        case 0x82:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v375);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v377);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v379);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_420260(v376, v378, v380, v381);
                            break;
                        case 0x83:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v382);
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v386);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_4202C0(v383, v384, v385, v387, v388);
                            break;
                        case 0x84:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v389);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_420310(v390, v391);
                            break;
                        case 0x86:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v392);
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_420350(v393, v394, v395);
                            break;
                        case 0x87:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v396);
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_420390(v397, v398, v399, v400, v401);
                            break;
                        case 0x88:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v402);
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_4203E0(v403, v404, v405);
                            break;
                        case 0x96:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v406);
                            token.AddParam(ReadInt32(_Pointer));
                            // sub_420440(v407, v408);
                            break;
                        case 0x97:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v409);
                            token.AddParam(ReadInt32(_Pointer));
                            // sub_420470(v410, v411);
                            break;
                        case 0x98:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v412);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_4204A0(v413, v414);
                            break;
                        case 0x99:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v415);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_4204E0(v416, v417);
                            break;
                        case 0x9A:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v418);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_420520(v419, v420);
                            break;
                        case 0x9B:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v421);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_420560(v422, v423);
                            break;
                        case 0x9E:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v424);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_4205A0(v425, v426);
                            break;
                        case 0x9F:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v427);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_4205E0(v428, v429);
                            break;
                        case 0x9C:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v430);
                            ReadInt32_takeLower16();
                            ReadInt32_takeLower16();
                            // sub_420620(v431);
                            break;
                        case 0x9D:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v432);
                            token.AddParam(ReadInt32_takeLower16());
                            ReadInt32_takeLower16();
                            ReadInt32_takeLower16();
                            ReadInt32_takeLower16();
                            // sub_420650(v433, v434);
                            break;
                        case 0xD2:
                            token.AddParam(ReadInt32(_Pointer));
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_420EB0(v435, v436);
                            break;
                        case 0xD3:
                            token.AddParam(ReadInt32(_Pointer));
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v438);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v440);
                            token.AddParam(ReadInt32_takeLower16());
                            //((void(__cdecl *)(int, int, int, int))loc_420EF0)(v437, v439, v441, v442);
                            break;
                        case 0xD4:
                            token.AddParam(ReadInt32(_Pointer));
                            // sub_420FA0(v443);
                            break;
                        case 0xD5:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v444);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v446);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_420C10(v445, v447, v448);
                            break;
                        case 0x73:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v449);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EC60(v450, v451);
                            break;
                        case 0x75:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v452);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41EC80(v453, v454);
                            break;
                        case 0x74:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v455);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_41ECB0(v456, v457);
                            break;
                        case 0xDD:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v458);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_420FD0(v459, v460);
                            break;
                        case 0xDF:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v461);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_421010(v462, v463);
                            break;
                        case 0xDE:

                            // sub_421070();
                            break;
                        case 0xDC:
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_4210B0(v464, v465, v466);
                            break;
                        case 0xE1:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v467);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v469);
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_421250(v468, v470, v471, v472, v473);
                            break;
                        case 0xFF:
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v474);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v476);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v478);
                            token.AddParam(ReadInt32_takeLower16());
                            // token.AddParam(v480);
                            token.AddParam(ReadInt32_takeLower16());
                            // sub_421F00(v475, v477, v479, v481, v482);
                            break;
                        default:
                            // sub_423600();
                            break;
                    }
                }

                yield return token;
            }
        }
    }
}
