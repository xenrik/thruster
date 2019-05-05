using System;
using UnityEngine;

public abstract class PIDController<T> {
	private Func<T, float, T> multiplier;
	private Func<T, float, T> divider;
	private Func<T, T, T> adder;
	private Func<T, T, T> subtracter;

	public float Kp = 0.2f;
	public float Ki = 0.05f;
	public float Kd = 1f;

	private T lastError;

	private T P;
	private T I;
	private T D;

	protected PIDController (Func<T, float, T> multipler, 
			Func<T, float, T> divider, 	Func<T, T, T> adder, Func<T, T, T> subtracter,
		float Kp, float Ki, float Kd) {
		this.Kp = Kp;
		this.Ki = Ki;
		this.Kd = Kd;

		this.multiplier = multipler;
		this.divider = divider;
		this.adder = adder;
		this.subtracter = subtracter;
	}

	public T Update(T error, float dt) {
		P = error;
		I = adder(I, multiplier(error, dt));
		D = divider(subtracter(P, lastError), dt);
		lastError = error;

		return adder(adder(multiplier(P, Kp), multiplier(I, Ki)), multiplier(D, Kd));
	}
}

[System.Serializable]
public class PIDControllerVector3 : PIDController<Vector3> {
	public PIDControllerVector3(float Kp = 0.2f, float Ki = 0.05f, float Kd = 1f) :
		base((a, b) => a * b, (a, b) => a / b, (a, b) => a + b, (a, b) => a - b,
			Kp, Ki, Kd) {
	}
}

[System.Serializable]
public class PIDControllerFloat : PIDController<float> {
	public PIDControllerFloat(float Kp = 0.2f, float Ki = 0.05f, float Kd = 1f) :
		base((a, b) => a * b, (a, b) => a / b, (a, b) => a + b, (a, b) => a - b,
			Kp, Ki, Kd) {
	}
}

