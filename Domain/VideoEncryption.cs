namespace PsVDecrypt.Domain
{
  public class VideoEncryption
  {
    private static bool useV1 = true;
    public static string string1_v2 = "pluralsight";
    public static string string2_v2 = "#©>Å£Q\x0005¤°";

    public static void XorBuffer(byte[] buff, int length, long position)
    {
      string str1 = "pluralsight";
      string str2 = "\x0006?zY¢\x00B2\x0085\x009FL\x00BEî0Ö.ì\x0017#©>Å£Q\x0005¤°\x00018Þ^\x008Eú\x0019Lqß'\x009D\x0003ßE\x009EM\x0080'x:\0~\x00B9\x0001ÿ 4\x00B3õ\x0003Ã§Ê\x000EAË\x00BC\x0090è\x009Eî~\x008B\x009Aâ\x001B¸UD<\x007FKç*\x001Döæ7H\v\x0015Arý*v÷%Âþ\x00BEä;pü";
      for (int index = 0; index < length; ++index)
      {
        byte num = (byte) ((ulong) ((int) str1[(int) ((position + (long) index) % (long) str1.Length)] ^ (int) str2[(int) ((position + (long) index) % (long) str2.Length)]) ^ (ulong) ((position + (long) index) % 251L));
        buff[index] = (byte) ((uint) buff[index] ^ (uint) num);
      }
    }

    public static void XorBufferV2(byte[] buff, int length, long position)
    {
      for (int index = 0; index < length; ++index)
      {
        byte num = (byte) ((ulong) ((int) VideoEncryption.string1_v2[(int) ((position + (long) index) % (long) VideoEncryption.string1_v2.Length)] ^ (int) VideoEncryption.string2_v2[(int) ((position + (long) index) % (long) VideoEncryption.string2_v2.Length)]) ^ (ulong) ((position + (long) index) % 251L));
        buff[index] = (byte) ((uint) buff[index] ^ (uint) num);
      }
    }

    public static void EncryptBuffer(byte[] buff, int length, long position)
    {
      VideoEncryption.XorBufferV2(buff, length, position);
    }

    public static void DecryptBuffer(byte[] buff, int length, long position)
    {
      if (position == 0L && length > 3)
      {
        VideoEncryption.XorBuffer(buff, length, position);
        if (buff[0] == (byte) 0 && buff[1] == (byte) 0 && buff[2] == (byte) 0)
        {
          VideoEncryption.useV1 = true;
        }
        else
        {
          VideoEncryption.XorBuffer(buff, length, position);
          VideoEncryption.XorBufferV2(buff, length, position);
          if (buff[0] == (byte) 0 && buff[1] == (byte) 0)
          {
            int num = (int) buff[2];
          }
          VideoEncryption.useV1 = false;
        }
      }
      else if (VideoEncryption.useV1)
        VideoEncryption.XorBuffer(buff, length, position);
      else
        VideoEncryption.XorBufferV2(buff, length, position);
    }
  }
}
