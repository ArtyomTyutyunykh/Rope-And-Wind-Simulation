using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class Rope : MonoBehaviour
{
    [SerializeField] private GameObject _connectionPoint;
    [SerializeField] private GameObject _load;
    [SerializeField] private float _loadMass = 1f;
    [SerializeField] private Wind _wind;

    // Лист точек троса
    private List<RopePoint> _ropePoints = new List<RopePoint>();

    // Характеристики троса
    private float _ropeLength = 5f;
    private int _numberOfPoints = 10;
    private float _ropeSectionLength;

    // Количество шагов для применения ограничений на точки
    private int _applyConstraintStepsCount = 25;

    // Буфер коллайдеров, с которым сталкивается груз
    private static int _colliderHitBufferSize = 10;
    private Collider[] colliderHitBuffer = new Collider[_colliderHitBufferSize];
    // Коллайдер груза
    private Collider _сollider;
    // Величина силы выталкивания груза
    private float _separateColliderValue = 0.1f;

    // Рендер троса
    private LineRenderer _lineRenderer;
    private float _lineWidth = 0.15f;

    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _сollider = _load.GetComponent<Collider>();

        // Создание троса
        _ropeSectionLength = _ropeLength / _numberOfPoints;
        Vector3 ropePointPosition = _connectionPoint.transform.position;
        for (int i = 0; i < _numberOfPoints - 1; i++)
        {
            _ropePoints.Add(new RopePoint(ropePointPosition, 1));

            ropePointPosition.y -= _ropeSectionLength;
        }
        _ropePoints.Add(new RopePoint(ropePointPosition, _loadMass));
    }

    private void Update()
    {
        // Отрисовка троса
        DrawRope();

        // Перемещение груза
        _load.transform.position = _ropePoints[_numberOfPoints - 1].currentPosition;

        // Вращение груза
        Vector3 PreLastPoint = _ropePoints[_numberOfPoints - 2].currentPosition;
        Vector3 LastPoint = _ropePoints[_numberOfPoints - 1].currentPosition;
        Vector3 direction = (PreLastPoint - LastPoint).normalized;
        _load.transform.rotation = Quaternion.LookRotation(direction, Vector3.forward);
    }

    private void FixedUpdate()
    {
        // Симуляция троса
        RopeSimulation();

        // Симуляция ветра
        ApplyWind();
    }

    // Алгоритм Верле для расчета положений точек
    private void RopeSimulation()
    {
        Vector3 gravityVector = new Vector3(0f, -9.81f, 0f);
        float t = Time.fixedDeltaTime;

        // Движение первой точки троса
        RopePoint firstRopePoint = _ropePoints[0];
        firstRopePoint.currentPosition = _connectionPoint.transform.position;
        _ropePoints[0] = firstRopePoint;

        // Вычисление новых положений точек
        for (int i = 1; i < _numberOfPoints; i++)
        {
            RopePoint currentRopePoint = _ropePoints[i];

            Vector3 velocity = currentRopePoint.currentPosition - currentRopePoint.oldPosition;
            currentRopePoint.oldPosition = currentRopePoint.currentPosition;
            currentRopePoint.currentPosition += velocity;
            currentRopePoint.currentPosition += gravityVector * currentRopePoint.mass * t * t;

            _ropePoints[i] = currentRopePoint;
        }

        // Применение ограничений на расстояние для всех точек и расчет столкновений груза
        for (int i = 0; i < _applyConstraintStepsCount; i++)
        {
            ApplyConstraint();

            ApplyCollisions();
        }
    }

    // Применение ограничений
    private void ApplyConstraint()
    {
        for (int i = 0; i < _numberOfPoints - 1; i++)
        {
            RopePoint topPoint = _ropePoints[i];
            RopePoint bottomPoint = _ropePoints[i + 1];

            float dist = (topPoint.currentPosition - bottomPoint.currentPosition).magnitude;
            float distError = Mathf.Abs(dist - _ropeSectionLength);

            // Расчет направления
            Vector3 changeDir = Vector3.zero;
            if (dist > _ropeSectionLength)
            {
                changeDir = (topPoint.currentPosition - bottomPoint.currentPosition).normalized;
            }
            else if (dist < _ropeSectionLength)
            {
                changeDir = (bottomPoint.currentPosition - topPoint.currentPosition).normalized;
            }

            // Применение ограничений
            Vector3 change = changeDir * distError;
            if (i != 0)
            {
                bottomPoint.currentPosition += change * 0.5f;
                _ropePoints[i + 1] = bottomPoint;

                topPoint.currentPosition -= change * 0.5f;
                _ropePoints[i] = topPoint;
            }
            else
            {
                bottomPoint.currentPosition += change;
                _ropePoints[i + 1] = bottomPoint;
            }
        }
    }

    // Расчет столкновений груза
    private void ApplyCollisions()
    {
        RopePoint LastPoint = _ropePoints[_numberOfPoints - 1];

        // Поиск коллайдеров, с которыми сталкивается груз
        int result = -1;
        result = Physics.OverlapBoxNonAlloc(LastPoint.currentPosition, _load.transform.localScale / 2, colliderHitBuffer);

        if (result > 0)
        {
            for (int n = 0; n < result; n++)
            {
                if (colliderHitBuffer[n].gameObject != _load)
                {
                    // Коллайдер с которым столкнулся груз
                    Vector3 colliderPosition = colliderHitBuffer[n].transform.position;
                    Quaternion colliderRotation = colliderHitBuffer[n].gameObject.transform.rotation;

                    Vector3 direction; // Направление для разделения коллайдеров
                    float distance; // Расстояние по направлению, необходимое для разделения коллайдеров

                    Physics.ComputePenetration(_сollider, LastPoint.currentPosition, _load.transform.rotation, colliderHitBuffer[n], colliderPosition, colliderRotation, out direction, out distance);

                    // Выталкиваем груз
                    LastPoint.currentPosition += direction * distance * _separateColliderValue;
                    _ropePoints[_numberOfPoints - 1] = LastPoint;
                }
            }
        }
    }

    // Расчет влияния ветра на груз
    private void ApplyWind()
    {
        RopePoint LastPoint = _ropePoints[_numberOfPoints - 1];
        float t = Time.fixedDeltaTime;

        Vector3 wind = _wind.GetWind();
        
        LastPoint.currentPosition += wind * t * t;
        _ropePoints[_numberOfPoints - 1] = LastPoint;
    }

    // Отрисовка троса
    private void DrawRope()
    {
        _lineRenderer.startWidth = _lineWidth;
        _lineRenderer.endWidth = _lineWidth;

        Vector3[] ropePointsPositions = new Vector3[_numberOfPoints];
        for (int i = 0; i < _numberOfPoints; i++)
        {
            ropePointsPositions[i] = _ropePoints[i].currentPosition;
        }
        _lineRenderer.positionCount = ropePointsPositions.Length;
        _lineRenderer.SetPositions(ropePointsPositions);
    }
}
