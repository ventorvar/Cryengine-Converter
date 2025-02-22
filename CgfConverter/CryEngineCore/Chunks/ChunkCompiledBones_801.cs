﻿using System.Collections.Generic;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    class ChunkCompiledBones_801 : ChunkCompiledBones
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);
            SkipBytes(b, 32);  // Padding between the chunk header and the first bone.

            //  Read the first bone with ReadCompiledBone, then recursively grab all the children for each bone you find.
            //  Each bone structure is 324 bytes, so will need to seek childOffset * 324 each time, and go back.
            NumBones = (int)((Size - 48) / 324);
            for (int i = 0; i < NumBones; i++)
            {
                var tempBone = new CompiledBone();
                tempBone.ReadCompiledBone_801(b);

                if (RootBone == null)  // First bone read is root bone
                    RootBone = tempBone;

                if (tempBone.offsetParent != 0)
                    tempBone.ParentBone = BoneList[i + tempBone.offsetParent];

                if (tempBone.ParentBone != null)
                    tempBone.parentID = tempBone.ParentBone.ControllerID;
                else
                    tempBone.parentID = 0;

                BoneList.Add(tempBone);
            }

            // Add the ChildID to the parent bone.  This will help with navigation. Also set up the TransformSoFar
            foreach (CompiledBone bone in BoneList)
            {
                AddChildIDToParent(bone);
            }
            SkinningInfo skin = GetSkinningInfo();
            skin.CompiledBones = new List<CompiledBone>();
            skin.HasSkinningInfo = true;
            skin.CompiledBones = BoneList;
        }
    }
}
