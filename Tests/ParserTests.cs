using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser;

namespace ParserTests
{
	[TestClass]
	public class ParserTests
	{
		[TestMethod] public void Test_1() => Run(1);
		[TestMethod] public void Test_2() => Run(2);
		[TestMethod] public void Test_3() => Run(3);
		[TestMethod] public void Test_4() => Run(4);
		[TestMethod] public void Test_5() => Run(5);
		[TestMethod] public void Test_6() => Run(6);
		[TestMethod] public void Test_7() => Run(7);
		[TestMethod] public void Test_8() => Run(8);
		[TestMethod] public void Test_9() => Run(9);
		[TestMethod] public void Test_10() => Run(10);
		[TestMethod] public void Test_11() => Run(11);
		[TestMethod] public void Test_12() => Run(12);
		[TestMethod] public void Test_13() => Run(13);
		[TestMethod] public void Test_14() => Run(14);
		[TestMethod] public void Test_15() => Run(15);
		[TestMethod] public void Test_16() => Run(16);
		[TestMethod] public void Test_17() => Run(17);
		[TestMethod] public void Test_18() => Run(18);
		[TestMethod] public void Test_19() => Run(19);
		[TestMethod] public void Test_20() => Run(20);
		[TestMethod] public void Test_21() => Run(21);
		[TestMethod] public void Test_22() => Run(22);
		[TestMethod] public void Test_23() => Run(23);
		[TestMethod] public void Test_24() => Run(24);
		[TestMethod] public void Test_25() => Run(25);
		[TestMethod] public void Test_26() => Run(26);
		[TestMethod] public void Test_27() => Run(27);
		[TestMethod] public void Test_28() => Run(28);
		[TestMethod] public void Test_29() => Run(29);
		[TestMethod] public void Test_30() => Run(30);
		[TestMethod] public void Test_31() => Run(31);
		[TestMethod] public void Test_32() => Run(32);
		[TestMethod] public void Test_33() => Run(33);
		[TestMethod] public void Test_34() => Run(34);
		[TestMethod] public void Test_35() => Run(35);
		[TestMethod] public void Test_36() => Run(36);
		[TestMethod] public void Test_37() => Run(37);
		[TestMethod] public void Test_38() => Run(38);
		[TestMethod] public void Test_39() => Run(39);
		[TestMethod] public void Test_40() => Run(40);
		[TestMethod] public void Test_41() => Run(41);
		[TestMethod] public void Test_42() => Run(42);
		[TestMethod] public void Test_43() => Run(43);
		[TestMethod] public void Test_44() => Run(44);
		[TestMethod] public void Test_45() => Run(45);
		[TestMethod] public void Test_46() => Run(46);
		[TestMethod] public void Test_47() => Run(47);
		[TestMethod] public void Test_48() => Run(48);
		[TestMethod] public void Test_49() => Run(49);
		[TestMethod] public void Test_50() => Run(50);
		[TestMethod] public void Test_51() => Run(51);
		[TestMethod] public void Test_52() => Run(52);
		[TestMethod] public void Test_53() => Run(53);
		[TestMethod] public void Test_54() => Run(54);
		[TestMethod] public void Test_55() => Run(55);
		[TestMethod] public void Test_56() => Run(56);
		[TestMethod] public void Test_57() => Run(57);
		[TestMethod] public void Test_58() => Run(58);
		[TestMethod] public void Test_59() => Run(59);
		[TestMethod] public void Test_60() => Run(60);
		[TestMethod] public void Test_61() => Run(61);
		[TestMethod] public void Test_62() => Run(62);
		[TestMethod] public void Test_63() => Run(63);
		[TestMethod] public void Test_64() => Run(64);
		[TestMethod] public void Test_65() => Run(65);
		[TestMethod] public void Test_66() => Run(66);
		[TestMethod] public void Test_67() => Run(67);
		[TestMethod] public void Test_68() => Run(68);
		[TestMethod] public void Test_69() => Run(69);
		[TestMethod] public void Test_70() => Run(70);
		[TestMethod] public void Test_71() => Run(71);
		[TestMethod] public void Test_72() => Run(72);
		[TestMethod] public void Test_73() => Run(73);
		[TestMethod] public void Test_74() => Run(74);
		[TestMethod] public void Test_75() => Run(75);
		[TestMethod] public void Test_76() => Run(76);
		[TestMethod] public void Test_77() => Run(77);
		[TestMethod] public void Test_78() => Run(78);
		[TestMethod] public void Test_79() => Run(79);
		[TestMethod] public void Test_80() => Run(80);
		[TestMethod] public void Test_81() => Run(81);
		[TestMethod] public void Test_82() => Run(82);
		[TestMethod] public void Test_83() => Run(83);
		[TestMethod] public void Test_84() => Run(84);
		[TestMethod] public void Test_85() => Run(85);
		[TestMethod] public void Test_86() => Run(86);
		[TestMethod] public void Test_87() => Run(87);
		[TestMethod] public void Test_88() => Run(88);
		[TestMethod] public void Test_89() => Run(89);
		[TestMethod] public void Test_90() => Run(90);
		[TestMethod] public void Test_91() => Run(91);
		[TestMethod] public void Test_92() => Run(92);
		[TestMethod] public void Test_93() => Run(93);
		[TestMethod] public void Test_94() => Run(94);
		[TestMethod] public void Test_95() => Run(95);
		[TestMethod] public void Test_96() => Run(96);
		[TestMethod] public void Test_97() => Run(97);
		[TestMethod] public void Test_98() => Run(98);
		[TestMethod] public void Test_99() => Run(99);
		[TestMethod] public void Test_100() => Run(100);
		[TestMethod] public void Test_101() => Run(101);
		[TestMethod] public void Test_102() => Run(102);
		[TestMethod] public void Test_103() => Run(103);
		[TestMethod] public void Test_104() => Run(104);
		[TestMethod] public void Test_105() => Run(105);
		[TestMethod] public void Test_106() => Run(106);
		[TestMethod] public void Test_107() => Run(107);
		[TestMethod] public void Test_108() => Run(108);
		[TestMethod] public void Test_109() => Run(109);
		[TestMethod] public void Test_110() => Run(110);
		[TestMethod] public void Test_111() => Run(111);
		[TestMethod] public void Test_112() => Run(112);
		[TestMethod] public void Test_113() => Run(113);
		[TestMethod] public void Test_114() => Run(114);
		[TestMethod] public void Test_115() => Run(115);
		[TestMethod] public void Test_116() => Run(116);
		[TestMethod] public void Test_117() => Run(117);
		[TestMethod] public void Test_118() => Run(118);
		[TestMethod] public void Test_119() => Run(119);
		[TestMethod] public void Test_120() => Run(120);
		[TestMethod] public void Test_121() => Run(121);
		[TestMethod] public void Test_122() => Run(122);
		[TestMethod] public void Test_123() => Run(123);
		[TestMethod] public void Test_124() => Run(124);
		[TestMethod] public void Test_125() => Run(125);
		[TestMethod] public void Test_126() => Run(126);
		[TestMethod] public void Test_127() => Run(127);
		[TestMethod] public void Test_128() => Run(128);
		[TestMethod] public void Test_129() => Run(129);
		[TestMethod] public void Test_130() => Run(130);
		[TestMethod] public void Test_131() => Run(131);

		private void Run(int testIndex)
		{
			//var sourcePath = Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName).Parent.FullName;
			var sourcePath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
			var path = Path.Combine(sourcePath, string.Format("ParserTests/ParserTest_{0}_in.txt", testIndex));
			Console.WriteLine("↓------------------Input----------------↓\n");
			var input = File.ReadAllText(path);
			Console.WriteLine(input);
			Console.WriteLine("\n↓------------------Output----------------↓\n");
			var parser = new Parser.Parser(path);
			var output = "";
			try {
				foreach (var v in parser.ParseProgram()) {
					output += v?.ToString() ?? "null\n";
				}
			} catch (ParserException e) {
				output += e.Message;
			}
			Console.WriteLine(output);
			var validOutput = File.ReadAllText(Path.Combine(
				sourcePath, string.Format("ParserTests/ParserTest_{0}_out.txt", testIndex)));
			Assert.IsTrue(output.Equals(validOutput, StringComparison.OrdinalIgnoreCase));
		}
	}
}
