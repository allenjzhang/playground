import java.util.*;
import java.lang.StringBuilder; 
/**
 * Generic implementation of the AVLTree.
 */
public class AVLTree<T extends Comparable<T>> {



    private Node root;      //reference to the root node of the tree 
    private int size;       //number of values stored in this tree 
    private Comparator<T> comparator;   //comparator object to overwrite the 
                                //natural ordering of the elements 
    	
	private boolean found;  //helper variable used by the remove methods
    private boolean added ; //helper variable used by the add method 

    /**
	 * Constructs a new, empty tree, sorted according to the natural ordering of its elements.
	 */
    public AVLTree () {
        root = null; 
        size = 0; 
        comparator = null; 
    }

    /**
	 * Constructs a new, empty tree, sorted according to the specified comparator.
	 */
    public AVLTree(Comparator<T> comparator) {
        this.comparator = comparator;
    }


    /**
     * Adds the specified element to this tree if it is not already present.
     * If this tree already contains the element, the call leaves the
     * tree unchanged and returns false.
     * @param data element to be added to this tree
     * @return true if this tree did not already contain the specified element
     * @throws NullPointerException if the specified element is null
     */
    public boolean add ( T data ) {
        added = false;
        if (data == null) return added;
        //replace root with the reference to the tree after the new
        //value is added
        root = add (data, root);
        //update the size and return the status accordingly
        if (added) size++;
        return added;
    }

    /*
     * Actual recursive implementation of add.
     *
     * This function returns a reference to the subtree in which
     * the new value was added.
     *
     * @param data element to be added to this tree
     * @param node node at which the recursive call is made
     */
    private Node add(T data, Node node) {
        if (node == null) {
            added = true;
            return new Node(data);
        }
        //decide how comparisons should be done
        int comp = 0 ;
        if (comparator == null ) //use natural ordering of the elements
            comp = node.data.compareTo(data);
        else                     //use the comparator
            comp = comparator.compare(node.data, data ) ;
        //find the location to add the new value
        if (comp > 0 ) { //add to the left subtree
            node.left = add(data, node.left);
        }
        else if (comp < 0 ) { //add to the right subtree
            node.right = add(data, node.right);
        }
        else { //duplicate found, do not add
            added = false;
            return node;
        }

        //update height of the node 
        updateHeight(node);

        //and rebalance if need be
        return rebalance(node); 
    }





    /**
	 * Removes the specified element from this tree if it is present. 
	 * Returns true if this tree contained the element (or equivalently, 
     * if this tree changed as a result of the call). 
     * (This tree will not contain the element once the call returns.)
	 * @param target object to be removed from this tree, if present
     * @return true if this set contained the specified element 
     * @throws NullPointerException if the specified element is null  
	 */
	public boolean remove(T target)
	{
        //replace root with a reference to the tree after target was removed 
		root = recRemove(target, root);
        //update the size and return the status accordingly 
		if (found) size--; 
		return found;
	}


	/*
	 * Actual recursive implementation of remove method: find the node to remove.
     *
	 * This function recursively finds and eventually removes the node with the target element 
     * and returns the reference to the modified tree to the caller. 
     * 
	 * @param target object to be removed from this tree, if present
     * @param node node at which the recursive call is made 
	 */
	private Node recRemove(T target, Node node)
	{
		if (node == null)  { //value not found 
			found = false;
            return node; 
        }
        
        //decide how comparisons should be done 
        int comp = 0 ;
        if (comparator == null ) //use natural ordering of the elements 
            comp = target.compareTo(node.data); 
        else                     //use the comparator 
            comp = comparator.compare(target, node.data ) ;

        
		if (comp < 0)       // target might be in a left subtree 
			node.left = recRemove(target, node.left);
		else if (comp > 0)  // target might be in a right subtree 
			node.right = recRemove(target, node.right );
		else {          // target found, now remove it 
			node = removeNode(node);
			found = true;
		}

        if (node == null ){     //don't try to rebalance null 
            return node; 
        }

        //update height of the node 
        updateHeight(node);

        //and rebalance if need be
        return rebalance(node); 
	}

	/*
	 * Actual recursive implementation of remove method: perform the removal.
	 *
	 * @param target the item to be removed from this tree
	 * @return a reference to the node itself, or to the modified subtree
	 */
	private Node removeNode(Node node)
	{
		T data;
		if (node.left == null)   //handle the leaf and one child node with right subtree 
			return node.right ; 
		else if (node.right  == null)  //handle one child node with left subtree 
			return node.left;
		else {                   //handle nodes with two children 
			data = getSuccessor(node.right);
			node.data = data;
			node.right = recRemove(data, node.right);
			return node;
		}
	}

	/*
	 * Returns the information held in the leftmost node of subtree
	 *
	 * @param subtree root of the subtree within which to search for the leftmost node
	 * @return returns data stored in the leftmost node of subtree
	 */
	private T getSuccessor(Node subtree)
	{
		if (subtree==null) //this should not happen 
            throw new NullPointerException("getSuccessor called with an empty subtree");
		Node temp = subtree;
		while (temp.left  != null)
			temp = temp.left ;
		return temp.data;
	}

	/**
	 * Returns the number of elements in this tree.
	 * @return the number of elements in this tree
	 */
	public int size() {
		return size;
	}

    public String toString() {
        StringBuilder sb = new StringBuilder(); 
        toStringInOrder(root, sb);
        if (sb.length() > 2 ) //remove the last ", "
            sb.setLength(sb.length()-2); 
        return "[" + sb.toString() + "]";
    }
    private void toStringInOrder(Node node, StringBuilder sb) {
        if (node == null ) return;
        toStringInOrder(node.left, sb);
        sb.append(node.toString()+ ", " ); 
        toStringInOrder(node.right, sb); 
    }



    private Node rebalance ( Node node ) { 
        
        updateHeight(node);
        
        int bf = balanceFactor(node);
        if( bf < -1 ) {
            int bfChild = balanceFactor(node.left); 
            if (bfChild <= 0){
                return LL(node) ;  //LL rotation 
            }
            else {
                return LR(node) ;  //LR rotation 
            }
        }
        else if(bf > 1) {
            int bfChild = balanceFactor(node.right) ; 
            if (bfChild >= 0) {
                return RR(node);  //RR rotation 
            }
            else {
                return RL(node); //RL rotation 
            }
        }
        else { //no rotation needed 
            return node; 
        }
    }

    /*
     * Returns the balanceFactor of node based
     * on children heights
     * @param Node to calculate balance factor of
     * @return int of balance factor
     */
    private int balanceFactor ( Node n ) {
        if(n == null) {
            throw new IllegalArgumentException("balanceFactor should never be called with null");
        }
        if ( n.right == null ) {
            return -n.height;
        }
        if ( n.left == null ) {
            return  n.height;
        }
        return n.right.height - n.left.height;
    }

    /*
     * Updates the height of nodes after addition
     * @param Node to  update height of
     */
    private void updateHeight ( Node n ) {
        // if node is a leaf
        if(n.left == null && n.right == null) {
            n.height = 0;
        }
        else if (n.left == null) {
            n.height = n.right.height + 1;
        }
        else if (n.right == null) {
            n.height = n.left.height + 1;
        }
        else {
            n.height = Math.max( n.right.height, n.left.height) + 1;
        }
    }

    /**
     * Returns root node of subtree after left rotation
     * has been done
     * @param Node to rotate
     * @return new root of subtree
     */
    private Node LL(Node A) {
        System.out.println("LL"+A);
        Node B = A.left;
        A.left = B.right;
        B.right = A;
        updateHeight(A);
        updateHeight(B);
        return B;
    }

    /**
     * Returns root node of subtree after right rotation
     * has been done
     * @param Node to rotate
     * @return new root of subtree
     */
    private Node RR(Node A) {
        System.out.println("RR"+A);
        Node B = A.right;
        A.right = B.left;
        B.left = A;
        updateHeight(A);
        updateHeight(B);
        return B;
    }

    /**
     * Returns root node of subtree after left
     * right rotation has been done
     * @param Node to rotate
     * @return new root of subtree
     */
    private Node LR(Node A) {
        System.out.println("LR"+A);
        Node B = A.left;
        Node C = B.right;
        A.left = C.right;
        B.right = C.left;
        C.left = B;
        C.right = A;
        updateHeight(A);
        updateHeight(B);
        updateHeight(C);
        return C;
    }

    /**
     * Returns root node of subtree after right
     * left rotation has been done
     * @param Node to rotate
     * @return new root of subtree
     */
    private Node RL(Node A) {
        System.out.println("RL"+A);
        Node B = A.right;
        Node C = B.left;
        A.right = C.left;
        B.left = C.right;
        C.right = B;
        C.left = A;
        updateHeight(A);
        updateHeight(B);
        updateHeight(C);
        return C;
    }






    public String toStringTree( ) {
        StringBuffer sb = new StringBuffer(); 
        toStringTree(sb, root, 0);
        return sb.toString();
    }
    //uses preorder traversal to display the tree 
    //WARNING: will not work if the data.toString returns more than one line 
    private void toStringTree( StringBuffer sb, Node node, int level ) {
        //display the node 
        if (level > 0 ) {
            for (int i = 0; i < level-1; i++) {
                sb.append("   ");
            }
            sb.append("|--");
        }
        if (node == null) {
            sb.append( "->\n"); 
            return;
        }
        else {
            sb.append( node.data + "\n"); 
        }

        //display the left subtree 
        toStringTree(sb, node.left, level+1); 
        //display the right subtree 
        toStringTree(sb, node.right, level+1); 
    }



    /*
    * Node class for this tree
    */
    private class Node implements Comparable < Node > {
        T data;
        int height;
        Node  left;
        Node  right;
        public Node ( T data ) {
            this.data = data;
            this.height = 0;
        }
        public Node (T data, Node left, Node right ) {
            this.data = data;
            this.height = 0; //not a good idea 
            this.left = left;
            this.right = right;
        }
        public int compareTo ( Node other ) {
            if (AVLTree.this.comparator == null )
                return this.data.compareTo ( other.data );
            else
                return comparator.compare(this.data, other.data);
        }
        public String toString() {
            return data.toString(); 
        }
    }


}

