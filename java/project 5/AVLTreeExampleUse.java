


public class AVLTreeExampleUse { 


    public static void main( String [] args ){


        AVLTree<Integer> tree = new AVLTree<>(new AbsValComparator() );

        tree.add(-1);
        tree.add(2);
        tree.add(-3);
        tree.add(4);
        tree.add(-5);
        tree.add(-6);
        tree.add(7);
        tree.add(-8);
        tree.add(9);
        tree.add(-10);

        

        System.out.println(tree.toStringTree() ); 
        System.out.println(tree.toString());

    }


}

