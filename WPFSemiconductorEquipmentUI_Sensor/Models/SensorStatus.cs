namespace WPFSemiconductorEquipmentUI_Sensor.Models
{
    // 센서의 도메인 상태. 표시 문구/색은 View 계층의 컨버터가 결정한다(MVVM 본격 분리).
    public enum SensorStatus
    {
        Idle,        // 아직 읽기 전(대기)
        Normal,      // 정상 범위
        Warning,     // 임계 초과(경고)
        Stale,       // 값이 갱신되지 않음(정지)
        AiCorrected  // AI 자동제어가 값을 임계 아래로 보정함
    }
}
