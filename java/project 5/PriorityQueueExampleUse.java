import java.util.ArrayList;
import java.util.Arrays;
import java.util.PriorityQueue;
import java.util.List;

public class PriorityQueueExampleUse {

	public static void main(String[] args) {

		Integer[] input = new Integer[] { 5, 40, 70, 20, 80, 100, 30 };
		List<Integer> in = Arrays.asList(input);
		// PriorityQueue<String> pets = new PriorityQueue<>( );
		PriorityQueue<Integer> pets = new PriorityQueue<Integer>(in);
		// PriorityQueue<Integer> pets = new PriorityQueue<Integer>(input);
		/*
		 * pets.add("fish");
		 * pets.add("rabbit");
		 * pets.add("dog");
		 * pets.add("cat");
		 * pets.add("turtle");
		 * pets.add("duck");
		 * pets.add("canary");
		 * pets.add("pig");
		 * 
		 * while (!pets.isEmpty()) {
		 * System.out.println(pets.remove());
		 * }
		 */
	}

}
