namespace FluxoCaixa.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa uma senha já criptografada/hasheada
    /// </summary>
    public class SenhaHash
    {
        public string Hash { get; private set; }

        protected SenhaHash() { }

        public SenhaHash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentException("O hash da senha não pode ser vazio", nameof(hash));

            Hash = hash;
        }

        public override bool Equals(object obj)
        {
            if (obj is SenhaHash senhaHash)
                return Hash == senhaHash.Hash;

            return false;
        }

        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }

        public override string ToString()
        {
            return "***"; // Nunca expor o hash
        }

        public static implicit operator string(SenhaHash senhaHash)
        {
            return senhaHash?.Hash;
        }
    }
}
