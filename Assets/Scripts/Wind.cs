using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
    // Направление и скорость ветра
    [SerializeField] private Vector3 _windDirection;
    [SerializeField] private float _windVelocity;

    // Ветровая нагрузка
    private float _windForce;

    // Вероятность изменения направления ветра
    // и период, через который может произойти изменение
    private static float _probabilityOfDirectionChange = 0.2f;
    private static float _directionChangePeriod = 2f;
    // Вероятность изменения скорости ветра и период
    private static float _probabilityOfVelocityChange = 0.1f;
    private static float _velocityChangePeriod = 2f;

    // Текущая величина случайного значения
    private float _tempPorbForDirection;
    private float _tempPorbForVelocity;

    // Величина (в градусах), на которое может измениться направление
    private float _windDirectionChangeLimit = 15;
    
    // Характеристики порывов ветра
    private float _windPulsationCoefficient = 1.3f; // Коэффициент пульсации
    private float _windPulsationPeriod = 3f;        // Период пульсации
    private bool _windFlaw = false;
    private float _oldWindVelocity;

    // Плотность воздуха
    private static float _airDensity = 1.25f;

    // Площадь проекции и коэффициент сопротивления объекта
    private float _objectProjectionArea;
    private float _objectDragCoefficient;

    private IEnumerator DirectionChange()
    {
        while (true)
        {
            _tempPorbForDirection = Random.Range(0f, 1f);
            yield return new WaitForSeconds(_directionChangePeriod);
        }
    }

    private IEnumerator VelocityChange()
    {
        while (true)
        {
            if (!_windFlaw)
            {
                _tempPorbForVelocity = Random.Range(0f, 1f);
                yield return new WaitForSeconds(_velocityChangePeriod);
            }
            else
            {
                yield return new WaitForSeconds(_windPulsationPeriod);
                _windVelocity = _oldWindVelocity;
                _windFlaw = false;
            }
        }
    }

    private void Start()
    {
        _windDirection = _windDirection.normalized;

        _objectProjectionArea = 1;      // для куба с размером 1       TODO сделать расчет площади проекции
        _objectDragCoefficient = 1.15f; // для грузов кубической формы TODO сделать выбор коэфициента от формы груза

        StartCoroutine(DirectionChange());
        StartCoroutine(VelocityChange());
    }

    private void Update()
    {
        ChangeWindDirection();
        ChangeWindVelocity();
        
        // Вычисляем ветровую нагрузку по формуле
        _windForce = 0.5f * _airDensity * _windVelocity * _objectProjectionArea * _objectDragCoefficient;
    }

    // Изменение направление ветра (в горизонтальной плоскости)
    private void ChangeWindDirection()
    {
        if (_tempPorbForDirection < _probabilityOfDirectionChange)
        {
            float leftOrRight = Random.Range(0f, 1f);
            if (leftOrRight > 0.5f)
            {
                _windDirection = Quaternion.AngleAxis(_windDirectionChangeLimit * Random.Range(0f, 1f), Vector3.up) * _windDirection;
            }
            else
            {
                _windDirection = Quaternion.AngleAxis(-1 *_windDirectionChangeLimit * Random.Range(0f, 1f), Vector3.up) * _windDirection;
            }
        }
    }

    // Изменение скорости ветра
    private void ChangeWindVelocity()
    {
        if (_tempPorbForVelocity < _probabilityOfVelocityChange && !_windFlaw)
        {
            _oldWindVelocity = _windVelocity;
            _windVelocity = _windVelocity * _windPulsationCoefficient;
            _windFlaw = true;
        }
    }

    public Vector3 GetWind()
    {
        Vector3 wind = _windDirection * _windForce;
        return wind;
    }
}
