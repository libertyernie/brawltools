using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrawlLib.SSBB {
	public class NodeDeepChecksum {
		public enum ResultType {
			SELF,
			CHILDREN_XOR,
			SELF_AND_CHILDREN_XOR
		};

		/// <summary>
		/// Find a checksum of this node's data, ensuring that changes in this
		/// node's children are reflected as well.
		/// </summary>
		/// <returns>1. An array of 16 bytes
		/// 2. The type of result - SELF, CHILDREN_XOR, or SELF_AND_CHILDREN_XOR</returns>
		/// <remarks>Three cases are possible:
		/// * CHILDREN_XOR: This node does not have a data block, and MD5() returns null - result is MD5ChildrenXor()
		/// * SELF: All children are included within the data block pointed to by OriginalSource -> result is MD5()
		/// * SELF_AND_CHILDREN_XOR: At least one child is not included within the data block -> result is MD5() xor with MD5ChildrenXor()</remarks>
		public static unsafe Tuple<byte[], ResultType> MD5EnsureChildrenIncluded(ResourceNode main) {
			byte[] self = main.MD5();
			if (self == null) {
				return new Tuple<byte[], ResultType>(MD5ChildrenXor(main), ResultType.CHILDREN_XOR);
			}
			if (!main.DataSourceContainsAllChildren()) {
				byte[] children = MD5ChildrenXor(main);
				for (int i = 0; i < 16; i++) self[i] ^= children[i];
				return new Tuple<byte[], ResultType>(self, ResultType.SELF_AND_CHILDREN_XOR);
			} else {
				return new Tuple<byte[], ResultType>(self, ResultType.SELF);
			}
		}

		/// <summary>
		/// Use an XOR operation to combine the results of running
		/// MD5EnsureChildrenIncluded on each child node.
		/// </summary>
		public static byte[] MD5ChildrenXor(ResourceNode main) {
			bool childrenfound = false;
			byte[] xorsum = new byte[16];
			foreach (ResourceNode node in main.Children) {
				byte[] md5 = MD5EnsureChildrenIncluded(main).Item1;
				if (md5 != null) {
					childrenfound = true;
					for (int i = 0; i < 16; i++) xorsum[i] ^= md5[i];
				}
			}
			return childrenfound ? xorsum : null;
		}

		/// <summary>
		/// Get the result of the MD5StrEnsureChildrenIncluded() function as a
		/// string of hexadecimal digits. If children's checksums were used in an
		/// XOR operation (in which case it's not a true MD5 checksum), a suffix
		/// will be appended to the string - either (c) or (s+c).
		/// If MD5StrEnsureChildrenIncluded() returns null, this method will
		/// return an empty string.
		/// </summary>
		[System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
		public static unsafe string MD5StrEnsureChildrenIncluded(ResourceNode main) {
			try {
				var r = MD5EnsureChildrenIncluded(main);
				byte[] checksum = r.Item1;
				if (checksum == null) return string.Empty;
				StringBuilder sb = new StringBuilder(checksum.Length * 2 + 5);
				for (int i = 0; i < checksum.Length; i++) {
					sb.Append(checksum[i].ToString("X2"));
				}
				if (r.Item2 == ResultType.CHILDREN_XOR) {
					sb.Append("(c)");
				} else if (r.Item2 == ResultType.SELF_AND_CHILDREN_XOR) {
					sb.Append("(s+c)");
				}
				return sb.ToString();
			} catch (AccessViolationException) {
				return "----AccessViolationException----";
			}
		}
	}
}
