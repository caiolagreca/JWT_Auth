using Microsoft.EntityFrameworkCore;

namespace JwtAuth.Data
{
    //DbContext é a classe base para trabalhar com o Entity Framework Core. ApplicationDbContext representa a sessão com o banco de dados e permite consultar e salvar dados.
    public class ApplicationDbContext : DbContext
    {
        //O construtor da classe aceita um parâmetro do tipo DbContextOptions<ApplicationDbContext> que contém as opções de configuração para o contexto do banco de dados, como a string de conexão e outras opções de configuração. O construtor chama o construtor da classe base (DbContext) passando options para que essas configurações sejam aplicadas ao DbContext.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Este método é sobrescrito da classe base DbContext e é usado para configurar o mapeamento das entidades para o banco de dados usando a API Fluent do Entity Framework. Isso significa que, dentro desse método, você pode definir regras e configurações específicas para o mapeamento das classes do modelo (entidades) para tabelas no banco de dados.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Esse comando aplica a configuração definida em uma classe separada, UserConfiguration. UserConfiguration é provavelmente uma classe que implementa a interface IEntityTypeConfiguration<User>, onde você pode definir configurações detalhadas para a entidade User, como chaves primárias, relacionamentos, restrições de campo, etc.
            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }

        //Esta propriedade representa a tabela Users no banco de dados. DbSet<T> é uma coleção que mapeia para uma tabela no banco de dados onde cada instância da entidade User representa uma linha na tabela Users. 
        public DbSet<User> Users { get; set; }

        //Similar à propriedade anterior, essa propriedade representa a tabela RefreshTokens no banco de dados. Cada instância da entidade RefreshToken corresponde a uma linha na tabela RefreshTokens.
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
