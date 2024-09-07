using ETICARET.DataAccess.Abstract;
using ETICARET.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETICARET.DataAccess.Concrete.EfCore
{
    public class EfCoreCartDal : EfCoreGenericRepository<Cart, DataContext>, ICartDal
    {
        public void ClearCart(string cartId)
        {
            throw new NotImplementedException();
        }

        public void DeleteFromCart(int cartId, int productId)
        {
            throw new NotImplementedException();
        }

        public Cart GetCartByUserId(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
