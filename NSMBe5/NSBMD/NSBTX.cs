﻿/*
*   This file is part of NSMB Editor 5.
*
*   NSMB Editor 5 is free software: you can redistribute it and/or modify
*   it under the terms of the GNU General Public License as published by
*   the Free Software Foundation, either version 3 of the License, or
*   (at your option) any later version.
*
*   NSMB Editor 5 is distributed in the hope that it will be useful,
*   but WITHOUT ANY WARRANTY; without even the implied warranty of
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*   GNU General Public License for more details.
*
*   You should have received a copy of the GNU General Public License
*   along with NSMB Editor 5.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using NSMBe5.DSFileSystem;

namespace NSMBe5.NSBMD
{
    public class NSBTX
    {
        File f;
        byte[] data;
        int texDataOffset;
        int palDataOffset;
        int palDefOffset;
        int palDataSize;
        int f5texDataOffset;
        int f5dataOffset;

        PalettedImage[] textures;
        PaletteDef[] palettes;
        public ByteArrayInputStream str;

        public NSBTX(File f)
        {
            this.f = f;

            data = f.getContents();
            str = new ByteArrayInputStream(data);

            //look for TEX0 block
            //ugly, but i'm lazy to implement it properly.
            bool found = false;
            int blockStart = 0;
            while (str.lengthAvailable(4))
            {
                uint v = str.readUInt();
                if (v == 0x30584554) // "TEX0"
                {
                    str.setOrigin(str.getPos()-4);
                    blockStart = (int)(str.getPos() - 4);
                    found = true;
                    break;
                }
//                else
//                    str.skipback(3); //just in case its not word-aligned
            }
            str.seek(0);
            if (!found)
            {
                textures = new PalettedImage[0];
                palettes = new PaletteDef[0];
                MessageBox.Show("This model file doesn't contain any valid texture data.", "NSMBe " + Version.GetString(), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Console.Out.WriteLine("\n");
            //Read stuff
            str.seek(0x14);
            texDataOffset = str.readInt() + blockStart;
            Console.Out.WriteLine("Texdata " + texDataOffset.ToString("X8"));

            str.seek(0x24);
            f5texDataOffset = str.readInt() + blockStart;
            Console.Out.WriteLine("f5Texdata " + f5texDataOffset.ToString("X8"));
            f5dataOffset = str.readInt() + blockStart;
            Console.Out.WriteLine("f5data " + f5dataOffset.ToString("X8"));
            
            str.seek(0x30);
            palDataSize = str.readInt() * 8;
            Console.Out.WriteLine("paldata size " + palDataSize.ToString("X8"));
            str.seek(0x34);
            palDefOffset = str.readInt();
            Console.Out.WriteLine("paldef " + palDefOffset.ToString("X8"));
            palDataOffset = str.readInt();
            Console.Out.WriteLine("paldata " + palDataOffset.ToString("X8"));

            //Read texture definitions
            str.seek(0x3D);
            textures = new PalettedImage[str.readByte()];
            str.skip((uint)(0xE + textures.Length * 4));

            ImageManagerWindow mgr = new ImageManagerWindow();
            mgr.Text = string.Format(LanguageManager.Get("ImageManager", "textureTitle"), f.name);

            bool hasFormat5 = false;
            for (int i = 0; i < textures.Length; i++)
            {
                int offset = 8 * str.readUShort();
                ushort param = str.readUShort();
                int format = (param >> 10) & 7;

                if (format == 5)
                    offset += f5texDataOffset;
                else
                    offset += texDataOffset;

                int width = 8 << ((param >> 4) & 7);
                int height = 8 << ((param >> 7) & 7);
                bool color0 = ((param >> 13) & 1) != 0;
                str.readUInt(); // unused

                int size = width*height*Image3D.bpps[format]/8;
                //Console.Out.WriteLine(offset.ToString("X8") + " " + format + " " + width + "x" + height + " " + color0 + " LZ");

                InlineFile mainfile = new InlineFile(f, offset, size, Image3D.formatNames[format]);
                if (format == 5)
                {
                    hasFormat5 = true;
                    int f5size = (width * height) / 16 * 2;
                    InlineFile f5file = new InlineFile(f, f5dataOffset, f5size, Image3D.formatNames[format]);

                    f5dataOffset += f5size;
                    textures[i] = new Image3Dformat5(mainfile, f5file, width, height);
                }
                else
                {
                    textures[i] = new Image3D(mainfile, color0, width, height, format);
                }

//                textures[i] = new Texture(this, color0, width, height, format, offset, "");
/*                if (format == 5)
                {
                    textures[i].f5DataOffset = f5dataOffset;
                    f5dataOffset += (uint)(width * height) / 16 * 2;
                }*/
            }

            for (int i = 0; i < textures.Length; i++)
            {
                if(textures[i] == null) continue;
                textures[i].name = str.ReadString(16);
                mgr.m.addImage(textures[i]);
            }



            //Read palette definitions
            str.seek(palDefOffset+1);
            palettes = new PaletteDef[str.readByte()];
            str.skip((uint)(0xE + palettes.Length * 4));

            for (int i = 0; i < palettes.Length; i++)
            {
                int offset = 8 * str.readUShort() + palDataOffset + blockStart;
                str.readUShort();
                palettes[i] = new PaletteDef();
                palettes[i].offs = offset;
            }

            Array.Sort(palettes);
         
            for (int i = 0; i < palettes.Length; i++)
            {
                palettes[i].name = str.ReadString(16);
                if (i != palettes.Length - 1)
                    palettes[i].size = palettes[i + 1].offs - palettes[i].offs;

            }
            palettes[palettes.Length - 1].size = blockStart+ palDataOffset + palDataSize - palettes[palettes.Length - 1].offs;

            for (int i = 0; i < palettes.Length; i++)
            {
                if (hasFormat5)
                {
                    FilePalette pa = new FilePalette(new InlineFile(f, palettes[i].offs, palettes[i].size, palettes[i].name));
                    mgr.m.addPalette(pa);
                }
                else
                {
                    int extrapalcount = (palettes[i].size) / 512;
                    for (int j = 0; j < extrapalcount; j++)
                    {
                        FilePalette pa = new FilePalette(new InlineFile(f, palettes[i].offs + j * 512, 512, palettes[i].name + ":" + j));
                        mgr.m.addPalette(pa);
                    }
                    int lastsize = palettes[i].size % 512;
                    if (lastsize != 0)
                    {
                        FilePalette pa = new FilePalette(new InlineFile(f, palettes[i].offs + extrapalcount * 512, lastsize, palettes[i].name + ":" + extrapalcount));
                        mgr.m.addPalette(pa);
                    }
                }
            }

            mgr.Show();

//            new ImagePreviewer(textures[0].render(palettes[0])).Show();
        }

        public void close()
        {
            f.endEdit(this);
        }

        public void save()
        {
            f.replace(data, this);
        }

        class PaletteDef : IComparable<PaletteDef>
        {
            public int offs, size;
            public string name;

            public int CompareTo(PaletteDef b)
            {
                return offs.CompareTo(b.offs);
            }
        }
    }
}
