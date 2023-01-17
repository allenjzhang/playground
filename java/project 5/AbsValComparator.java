
import java.util.*; 

/**
 * Provides comparison of Integer objects by their absolute value.
 */
public class AbsValComparator implements Comparator<Integer> {
    public int compare(Integer obj1, Integer obj2) {
        int val1 = obj1.intValue() * obj1.intValue(); 
        int val2 = obj2.intValue() * obj2.intValue(); 
        return val1 - val2; 
    }
}

