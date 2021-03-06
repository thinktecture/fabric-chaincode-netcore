// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: msp/identities.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbr = global::Google.Protobuf.Reflection;

namespace Chaincode.NET.Protos {

  /// <summary>Holder for reflection information generated from msp/identities.proto</summary>
  public static partial class IdentitiesReflection {

    #region Descriptor
    /// <summary>File descriptor for msp/identities.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static IdentitiesReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChRtc3AvaWRlbnRpdGllcy5wcm90bxIDbXNwIjUKElNlcmlhbGl6ZWRJZGVu",
            "dGl0eRINCgVtc3BpZBgBIAEoCRIQCghpZF9ieXRlcxgCIAEoDCJfChhTZXJp",
            "YWxpemVkSWRlbWl4SWRlbnRpdHkSDAoETnltWBgBIAEoDBIMCgROeW1ZGAIg",
            "ASgMEgoKAk9VGAMgASgMEgwKBFJvbGUYBCABKAwSDQoFUHJvb2YYBSABKAxC",
            "TQohb3JnLmh5cGVybGVkZ2VyLmZhYnJpYy5wcm90b3MubXNwWihnaXRodWIu",
            "Y29tL2h5cGVybGVkZ2VyL2ZhYnJpYy9wcm90b3MvbXNwYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Chaincode.NET.Protos.SerializedIdentity), global::Chaincode.NET.Protos.SerializedIdentity.Parser, new[]{ "Mspid", "IdBytes" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Chaincode.NET.Protos.SerializedIdemixIdentity), global::Chaincode.NET.Protos.SerializedIdemixIdentity.Parser, new[]{ "NymX", "NymY", "OU", "Role", "Proof" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  /// <summary>
  /// This struct represents an Identity
  /// (with its MSP identifier) to be used
  /// to serialize it and deserialize it
  /// </summary>
  public sealed partial class SerializedIdentity : pb::IMessage<SerializedIdentity> {
    private static readonly pb::MessageParser<SerializedIdentity> _parser = new pb::MessageParser<SerializedIdentity>(() => new SerializedIdentity());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<SerializedIdentity> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Chaincode.NET.Protos.IdentitiesReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SerializedIdentity() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SerializedIdentity(SerializedIdentity other) : this() {
      mspid_ = other.mspid_;
      idBytes_ = other.idBytes_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SerializedIdentity Clone() {
      return new SerializedIdentity(this);
    }

    /// <summary>Field number for the "mspid" field.</summary>
    public const int MspidFieldNumber = 1;
    private string mspid_ = "";
    /// <summary>
    /// The identifier of the associated membership service provider
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Mspid {
      get { return mspid_; }
      set {
        mspid_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "id_bytes" field.</summary>
    public const int IdBytesFieldNumber = 2;
    private pb::ByteString idBytes_ = pb::ByteString.Empty;
    /// <summary>
    /// the Identity, serialized according to the rules of its MPS
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pb::ByteString IdBytes {
      get { return idBytes_; }
      set {
        idBytes_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as SerializedIdentity);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(SerializedIdentity other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Mspid != other.Mspid) return false;
      if (IdBytes != other.IdBytes) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Mspid.Length != 0) hash ^= Mspid.GetHashCode();
      if (IdBytes.Length != 0) hash ^= IdBytes.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Mspid.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Mspid);
      }
      if (IdBytes.Length != 0) {
        output.WriteRawTag(18);
        output.WriteBytes(IdBytes);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Mspid.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Mspid);
      }
      if (IdBytes.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeBytesSize(IdBytes);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(SerializedIdentity other) {
      if (other == null) {
        return;
      }
      if (other.Mspid.Length != 0) {
        Mspid = other.Mspid;
      }
      if (other.IdBytes.Length != 0) {
        IdBytes = other.IdBytes;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            Mspid = input.ReadString();
            break;
          }
          case 18: {
            IdBytes = input.ReadBytes();
            break;
          }
        }
      }
    }

  }

  /// <summary>
  /// This struct represents an Idemix Identity
  /// to be used to serialize it and deserialize it.
  /// The IdemixMSP will first serialize an idemix identity to bytes using
  /// this proto, and then uses these bytes as id_bytes in SerializedIdentity
  /// </summary>
  public sealed partial class SerializedIdemixIdentity : pb::IMessage<SerializedIdemixIdentity> {
    private static readonly pb::MessageParser<SerializedIdemixIdentity> _parser = new pb::MessageParser<SerializedIdemixIdentity>(() => new SerializedIdemixIdentity());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<SerializedIdemixIdentity> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Chaincode.NET.Protos.IdentitiesReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SerializedIdemixIdentity() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SerializedIdemixIdentity(SerializedIdemixIdentity other) : this() {
      nymX_ = other.nymX_;
      nymY_ = other.nymY_;
      oU_ = other.oU_;
      role_ = other.role_;
      proof_ = other.proof_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SerializedIdemixIdentity Clone() {
      return new SerializedIdemixIdentity(this);
    }

    /// <summary>Field number for the "NymX" field.</summary>
    public const int NymXFieldNumber = 1;
    private pb::ByteString nymX_ = pb::ByteString.Empty;
    /// <summary>
    /// NymX is the X-component of the pseudonym elliptic curve point.
    /// It is a []byte representation of an amcl.BIG
    /// The pseudonym can be seen as a public key of the identity, it is used to verify signatures.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pb::ByteString NymX {
      get { return nymX_; }
      set {
        nymX_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "NymY" field.</summary>
    public const int NymYFieldNumber = 2;
    private pb::ByteString nymY_ = pb::ByteString.Empty;
    /// <summary>
    /// NymY is the Y-component of the pseudonym elliptic curve point.
    /// It is a []byte representation of an amcl.BIG
    /// The pseudonym can be seen as a public key of the identity, it is used to verify signatures.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pb::ByteString NymY {
      get { return nymY_; }
      set {
        nymY_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "OU" field.</summary>
    public const int OUFieldNumber = 3;
    private pb::ByteString oU_ = pb::ByteString.Empty;
    /// <summary>
    /// OU contains the organizational unit of the idemix identity
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pb::ByteString OU {
      get { return oU_; }
      set {
        oU_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "Role" field.</summary>
    public const int RoleFieldNumber = 4;
    private pb::ByteString role_ = pb::ByteString.Empty;
    /// <summary>
    /// Role contains the role of this identity (e.g., ADMIN or MEMBER)
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pb::ByteString Role {
      get { return role_; }
      set {
        role_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "Proof" field.</summary>
    public const int ProofFieldNumber = 5;
    private pb::ByteString proof_ = pb::ByteString.Empty;
    /// <summary>
    /// Proof contains the cryptographic evidence that this identity is valid
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pb::ByteString Proof {
      get { return proof_; }
      set {
        proof_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as SerializedIdemixIdentity);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(SerializedIdemixIdentity other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (NymX != other.NymX) return false;
      if (NymY != other.NymY) return false;
      if (OU != other.OU) return false;
      if (Role != other.Role) return false;
      if (Proof != other.Proof) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (NymX.Length != 0) hash ^= NymX.GetHashCode();
      if (NymY.Length != 0) hash ^= NymY.GetHashCode();
      if (OU.Length != 0) hash ^= OU.GetHashCode();
      if (Role.Length != 0) hash ^= Role.GetHashCode();
      if (Proof.Length != 0) hash ^= Proof.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (NymX.Length != 0) {
        output.WriteRawTag(10);
        output.WriteBytes(NymX);
      }
      if (NymY.Length != 0) {
        output.WriteRawTag(18);
        output.WriteBytes(NymY);
      }
      if (OU.Length != 0) {
        output.WriteRawTag(26);
        output.WriteBytes(OU);
      }
      if (Role.Length != 0) {
        output.WriteRawTag(34);
        output.WriteBytes(Role);
      }
      if (Proof.Length != 0) {
        output.WriteRawTag(42);
        output.WriteBytes(Proof);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (NymX.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeBytesSize(NymX);
      }
      if (NymY.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeBytesSize(NymY);
      }
      if (OU.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeBytesSize(OU);
      }
      if (Role.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeBytesSize(Role);
      }
      if (Proof.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeBytesSize(Proof);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(SerializedIdemixIdentity other) {
      if (other == null) {
        return;
      }
      if (other.NymX.Length != 0) {
        NymX = other.NymX;
      }
      if (other.NymY.Length != 0) {
        NymY = other.NymY;
      }
      if (other.OU.Length != 0) {
        OU = other.OU;
      }
      if (other.Role.Length != 0) {
        Role = other.Role;
      }
      if (other.Proof.Length != 0) {
        Proof = other.Proof;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            NymX = input.ReadBytes();
            break;
          }
          case 18: {
            NymY = input.ReadBytes();
            break;
          }
          case 26: {
            OU = input.ReadBytes();
            break;
          }
          case 34: {
            Role = input.ReadBytes();
            break;
          }
          case 42: {
            Proof = input.ReadBytes();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
