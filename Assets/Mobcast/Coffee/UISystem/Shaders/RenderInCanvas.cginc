struct appdata_t {
    float4 vertex : POSITION;
    fixed4 color : COLOR;
    float2 texcoord : TEXCOORD0;
};

struct v2f {
    float4 vertex : SV_POSITION;
    fixed4 color : COLOR;
    float2 texcoord : TEXCOORD0;
    float3 wpos : TEXCOORD1;
};

fixed _Alpha;
float4 _ClipRect_0;
float4 _ClipRect_1;
float4 _ClipRect_2;
float4 _ClipRect_3;

v2f vert (appdata_t v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.color = v.color;
    o.texcoord = v.texcoord;
    o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
    return o;
}

fixed4 ClipWorldRect (fixed4 col, float3 wpos)
{
    col.a *=  (wpos.x >= _ClipRect_0.x ) * (wpos.x <= _ClipRect_0.y ) * (wpos.y >= _ClipRect_0.z ) * (wpos.y <= _ClipRect_0.w )
            * (wpos.x >= _ClipRect_1.x ) * (wpos.x <= _ClipRect_1.y ) * (wpos.y >= _ClipRect_1.z ) * (wpos.y <= _ClipRect_1.w )
            * (wpos.x >= _ClipRect_2.x ) * (wpos.x <= _ClipRect_2.y ) * (wpos.y >= _ClipRect_2.z ) * (wpos.y <= _ClipRect_2.w )
            * (wpos.x >= _ClipRect_3.x ) * (wpos.x <= _ClipRect_3.y ) * (wpos.y >= _ClipRect_3.z ) * (wpos.y <= _ClipRect_3.w );
    return col;
}

fixed4 ApplyAlpha(fixed4 col)
{
    col.a *= _Alpha;
    clip(col.a - 0.01);
    return col;
}