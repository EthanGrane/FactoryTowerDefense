using UnityEngine;

/*
 *  Permite RECIBIR items a su inventario
 */
public interface IItemAcceptor
{
    bool CanAccept(Item item);
    bool Insert(Item item);
    
}
