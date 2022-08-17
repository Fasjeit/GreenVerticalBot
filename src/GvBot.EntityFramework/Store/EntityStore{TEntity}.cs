namespace GvBot.EntityFramework.Store
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Класс предоставляет базовый функционал по работе с сущностями EF.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    internal class EntityStore<TEntity>
        where TEntity : class, IBaseEntity
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EntityStore{TEntity}"/> на основе контекста БД.
        /// </summary>
        /// <param name="context">Контекст базы данных.</param>
        public EntityStore(DbContext context)
        {
            this.Context = context;
            this.DbEntitySet = context.Set<TEntity>();
        }

        /// <summary>
        /// Получает контекст БД.
        /// </summary>
        public DbContext Context { get; private set; }

        /// <summary>
        /// Получает коллекцию сущностей.
        /// </summary>
        public IQueryable<TEntity> EntitySet => this.DbEntitySet;

        /// <summary>
        /// Получает коллекцию сущностей.
        /// </summary>
        public IQueryable<TEntity> EntitySetNoTracking => this.DbEntitySet.AsNoTracking();

        /// <summary>
        /// Получает набор сущностей для данного хранилища.
        /// </summary>
        public DbSet<TEntity> DbEntitySet { get; private set; }

        /// <summary>
        /// Поиск сущности в хранилище по идентификатору.
        /// </summary>
        /// <param name="id">
        /// Идентификатор сущности.
        /// </param>
        /// <param name="noTracking">
        /// Не отслеживать изменения в сущности
        /// </param>
        /// <returns>
        /// Сущность с заданным идентификатором; null, если сущность не найдена.
        /// </returns>
        /// <remarks>
        /// noTracking должен быть выставлен в true для всех операций, для оъектов которых не будет производится обновление данных объектов.
        ///
        /// Обновление noTracking сущностей
        /// var blog = store.GetByIdAsync(id, noTracking : true);
        /// blog.Rating = 5;
        /// store.Update(blog);
        /// context.SaveChanges();
        ///
        /// Обновление traking сущностей:
        /// var blog = store.GetByIdAsync(id, noTracking : false);
        /// blog.Rating = 5;
        /// context.SaveChanges();
        /// </remarks>
        public virtual ValueTask<TEntity> GetByIdAsync(object id, bool noTracking = true)
        {
            if (noTracking)
            {
                return new ValueTask<TEntity>(this.DbEntitySet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id as string));
            }
            return this.DbEntitySet.FindAsync(id);
        }

        public virtual TEntity GetById(object id, bool noTracking = true)
        {
            if (noTracking)
            {
                return this.DbEntitySet.AsNoTracking().FirstOrDefault(e => e.Id == id as string);
            }
            return this.DbEntitySet.Find(id);
        }

        /// <summary>
        /// Создание новой сущности в хранилище.
        /// </summary>
        /// <param name="entity">Новая сущность.</param>
        public void Create(TEntity entity)
        {
            this.DbEntitySet.Add(entity);
        }

        /// <summary>
        /// Удаление сущности из хранилища.
        /// </summary>
        /// <param name="entity">Удаляемая сущность.</param>
        public void Delete(TEntity entity)
        {
            this.DbEntitySet.Remove(entity);
        }

        /// <summary>
        /// Удаление сущности из хранилища.
        /// </summary>
        /// <param name="entity">Удаляемая сущность.</param>
        public void Delete(string id)
        {
            var existedEntity = this.DbEntitySet.Find(id);
            if (existedEntity != null)
            {
                this.DbEntitySet.Remove(existedEntity);
            }
        }

        /// <summary>
        /// Удаление сущности из хранилища.
        /// </summary>
        /// <param name="entity">Удаляемая сущность.</param>
        public void Delete(params string[] keys)
        {
            var existedEntity = this.DbEntitySet.Find(keys);
            if (existedEntity != null)
            {
                this.DbEntitySet.Remove(existedEntity);
            }
        }

        public void Delete(IEnumerable<TEntity> entities)
        {
            this.DbEntitySet.RemoveRange(entities);
        }

        /// <summary>
        /// Обновление на основе no-traking сущности в хранилище.
        /// </summary>
        /// <param name="entity">
        /// Обновляемая сущность.
        /// </param>
        /// <remarks>
        /// Создаём tracking сущность, заполняем ей поля на основе переданной noTracking сущности
        /// </remarks>
        public virtual void Update(TEntity entity)
        {
            var existedEntity = this.DbEntitySet.Find(entity.Id);
            if (existedEntity != null)
            {
                this.Context.Entry(existedEntity).CurrentValues.SetValues(entity);
            }
        }
    }
}