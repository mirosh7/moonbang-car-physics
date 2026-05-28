/*
 * math_util.h - engine-independent vector math and helpers.
 *
 * Mirrors the small subset of UnityEngine.Mathf / Vector3 used by the
 * original C# models so the ported formulas stay identical.
 */
#ifndef CARSIM_MATH_UTIL_H
#define CARSIM_MATH_UTIL_H

#include <cmath>
#include <vector>
#include "car_physics.h"

namespace carsim {

constexpr float PI        = 3.14159265358979323846f;
constexpr float RPM_TO_RAD = PI * 2.0f / 60.0f;
constexpr float RAD_TO_RPM = 1.0f / RPM_TO_RAD;
constexpr float DEG2RAD    = PI / 180.0f;
constexpr float RAD2DEG    = 180.0f / PI;

struct Vec3 {
    float x = 0.f, y = 0.f, z = 0.f;

    Vec3() = default;
    Vec3(float x_, float y_, float z_) : x(x_), y(y_), z(z_) {}
    Vec3(const CP_Vec3& v) : x(v.x), y(v.y), z(v.z) {}

    CP_Vec3 toC() const { return CP_Vec3{ x, y, z }; }
};

inline Vec3 operator+(const Vec3& a, const Vec3& b) { return { a.x + b.x, a.y + b.y, a.z + b.z }; }
inline Vec3 operator-(const Vec3& a, const Vec3& b) { return { a.x - b.x, a.y - b.y, a.z - b.z }; }
inline Vec3 operator*(const Vec3& a, float s)       { return { a.x * s, a.y * s, a.z * s }; }
inline Vec3 operator*(float s, const Vec3& a)       { return a * s; }

inline float dot(const Vec3& a, const Vec3& b) { return a.x * b.x + a.y * b.y + a.z * b.z; }
inline float magnitude(const Vec3& a)          { return std::sqrt(dot(a, a)); }

inline Vec3 normalized(const Vec3& a) {
    float m = magnitude(a);
    return m > 1e-8f ? a * (1.0f / m) : Vec3{ 0.f, 0.f, 0.f };
}

/* Unity's Vector3.ProjectOnPlane(v, n) = v - n * (dot(v,n)/dot(n,n)). */
inline Vec3 projectOnPlane(const Vec3& v, const Vec3& n) {
    float nn = dot(n, n);
    if (nn < 1e-12f) return v;
    return v - n * (dot(v, n) / nn);
}

/* Unity's Transform.InverseTransformDirection for an orthonormal basis:
 * express world direction v in the local (right, up, forward) frame. */
inline Vec3 inverseTransformDirection(const Vec3& v, const Vec3& right,
                                      const Vec3& up, const Vec3& forward) {
    return { dot(v, right), dot(v, up), dot(v, forward) };
}

/* Unity Mathf helpers. */
inline float clampf(float v, float lo, float hi) {
    return v < lo ? lo : (v > hi ? hi : v);
}

inline float clamp01(float v) { return clampf(v, 0.0f, 1.0f); }

inline float lerp(float a, float b, float t) { return a + (b - a) * clamp01(t); }

inline float inverseLerp(float a, float b, float v) {
    if (a == b) return 0.0f;
    return clamp01((v - a) / (b - a));
}

/* Mathf.Sign: returns +1 for v >= 0, -1 otherwise. */
inline float signf(float v) { return v >= 0.0f ? 1.0f : -1.0f; }

inline float mapRangeClamped(float value, float inA, float inB, float outA, float outB) {
    return lerp(outA, outB, inverseLerp(inA, inB, value));
}

/* Owns its keyframes; evaluates with clamped linear interpolation. */
class Curve {
public:
    Curve() = default;

    void assign(const CP_Curve& c) {
        m_times.clear();
        m_values.clear();
        if (c.count > 0 && c.times && c.values) {
            m_times.assign(c.times, c.times + c.count);
            m_values.assign(c.values, c.values + c.count);
        }
        m_max = 0.0f;
        bool first = true;
        for (float v : m_values) {
            if (first || v > m_max) { m_max = v; first = false; }
        }
    }

    float evaluate(float t) const {
        if (m_values.empty()) return 0.0f;
        if (t <= m_times.front()) return m_values.front();
        if (t >= m_times.back())  return m_values.back();
        for (size_t i = 0; i + 1 < m_times.size(); ++i) {
            if (t >= m_times[i] && t <= m_times[i + 1]) {
                float span = m_times[i + 1] - m_times[i];
                float u = span > 1e-12f ? (t - m_times[i]) / span : 0.0f;
                return m_values[i] + (m_values[i + 1] - m_values[i]) * u;
            }
        }
        return m_values.back();
    }

    float maxValue() const { return m_max; }

private:
    std::vector<float> m_times;
    std::vector<float> m_values;
    float              m_max = 0.0f;
};

} // namespace carsim

#endif // CARSIM_MATH_UTIL_H
