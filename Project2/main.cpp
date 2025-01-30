#include <GL/glew.h>
#include <SFML/Graphics.hpp>
#include <SFML/OpenGL.hpp>
#include <SFML/Window.hpp>
#include <iostream>
#include <iomanip>
#include <cmath>
#include <ctime>
#include "SOIL.h"

#include <fstream>
#include <sstream>
#include <string>
#include <vector>

#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/type_ptr.hpp>

using namespace std;

// ID шейдерной программы
GLuint Program;
// ID атрибута
GLint coordAttribID;
GLint textureCoordAttribID;
GLint normalAttribID;

//GLint textureUniformID;

GLint transformModelUniformID;
GLint transformViewProjectionUniformID;
GLint transformNormalUniformID;
GLint transformViewPositionUniformID;

GLint materialTextureUniformID;
GLint materialAmbientUniformID;
GLint materialDiffuseUniformID;
GLint materialSpecularUniformID;
GLint materialEmissionUniformID;
GLint materialShininessUniformID;

GLint enableLightsUniformID;
GLint lightningModelUniformID;

GLint pointLightpositionUniformID;
GLint pointLightAmbientUniformID;
GLint pointLightDiffuseUniformID;
GLint pointLightSpecularUniformID;
GLint pointLightAttenuationUniformID;

GLint directionalLightDirectionUniformID;
GLint directionalLightAmbientUniformID;
GLint directionalLightDiffuseUniformID;
GLint directionalLightSpecularUniformID;

GLint spotLightPositionUniformID;
GLint spotLightAmbientUniformID;
GLint spotLightDiffuseUniformID;
GLint spotLightSpecularUniformID;
GLint spotLightAttenuationUniformID;
GLint spotLightSpotDirectionUniformID;
GLint spotLightSpotCutoffUniformID;
GLint spotLightSpotExponentUniformID;


// ID Vertex Buffer Object
GLuint VBO0;
GLuint VBO1;
GLuint VBO2;
GLuint VBO3;
GLuint VBO4;

struct Vertex {
    GLfloat x;
    GLfloat y;
    GLfloat z;
};

struct Color {
    GLfloat r;
    GLfloat g;
    GLfloat b;
};

struct TextureCoordinate {
    GLfloat x;
    GLfloat y;
};

struct Normal {
    GLfloat x;
    GLfloat y;
    GLfloat z;
};

struct Vertex_Texture {
    Vertex v;
    TextureCoordinate t;
};

struct Vertex_Texture_Normal {
    Vertex v;
    TextureCoordinate t;
    Normal n;
};

struct PolygonIndices {
    int vertexIndex;
    int textureCoordIndex;
    int normalIndex;
};

struct obj {
    vector<Vertex> verteces;
    vector<TextureCoordinate> textureCoords;
    vector<vector<PolygonIndices>> polygons;
    vector<Normal> normals;
};

const GLfloat Pi = 3.14159274101257324219f;

const char* VertexShaderSource = R"(
    #version 330 core
    in vec3 position;
    in vec2 texcoord;
    in vec3 normal;

    uniform struct Transform {
        mat4 model;
        mat4 viewProjection;
        mat3 normal;
        vec3 viewPosition;
    } transform;

    uniform struct PointLight {
        vec4 position;
        vec4 ambient;
        vec4 diffuse;
        vec4 specular;
        vec3 attenuation;
    } pointLight;

    uniform struct DirLight {
        vec4 direction;
        vec4 ambient;
        vec4 diffuse;
        vec4 specular;
    } dirLight;

    uniform struct SpotLight {
        vec4 position;
        vec4 ambient;
        vec4 diffuse;
        vec4 specular;
        vec3 attenuation;
        vec3 spotDirection;
        float spotCutoff;
        float spotExponent;
    } spotLight;

    out Vertex{
        vec2 texcoord;
        vec3 normal;
        vec3 viewDir;

        vec3 pointLightDir;
        vec3 dirLightDir;
        vec3 spotLightDir;
        
        float pointDistance;
        float spotDistance;
    } Vert;

    void main() {
        vec4 vertex = transform.model * vec4(position, 1.0);
        gl_Position = transform.viewProjection * vertex;
        
        Vert.texcoord = vec2(texcoord.x, 1.0f - texcoord.y);
        
        Vert.normal = transform.normal * normal;
        Vert.viewDir = transform.viewPosition - vec3(vertex);

        Vert.pointLightDir = vec3(pointLight.position - vertex);
        Vert.pointDistance = length(Vert.pointLightDir);

        
        Vert.dirLightDir = vec3(dirLight.direction);

        Vert.spotLightDir = vec3(spotLight.position - vertex);
        Vert.spotDistance = length(Vert.spotLightDir);
        
    }
)";

#define A(x) #x

// Исходный код фрагментного шейдера
const char* FragShaderSource = R"(
    #version 330 core
    in Vertex{
        vec2 texcoord;
        vec3 normal;
        vec3 viewDir;

        vec3 pointLightDir;
        vec3 dirLightDir;
        vec3 spotLightDir;
        
        float pointDistance;
        float spotDistance;
    } Vert;

    uniform vec3 enableLights;
    uniform int lightningModel;

    uniform struct PointLight {
        vec4 position;
        vec4 ambient;
        vec4 diffuse;
        vec4 specular;
        vec3 attenuation;
    } pointLight;

    uniform struct DirLight {
        vec4 direction;
        vec4 ambient;
        vec4 diffuse;
        vec4 specular;
    } dirLight;

    uniform struct SpotLight {
        vec4 position;
        vec4 ambient;
        vec4 diffuse;
        vec4 specular;
        vec3 attenuation;
        vec3 spotDirection;
        float spotCutoff;
        float spotExponent;
    } spotLight;

    uniform struct Material {
        sampler2D texture;
        vec4 ambient;
        vec4 diffuse;
        vec4 specular;
        vec4 emission;
        float shininess;
    } material;

    out vec4 color;

    void main() {
        vec3 normal = normalize(Vert.normal);
        vec3 pointLightDir = normalize(Vert.pointLightDir);
        vec3 dirLightDir = normalize(Vert.dirLightDir);
        vec3 spotLightDir = normalize(Vert.spotLightDir);
        vec3 viewDir = normalize(Vert.viewDir);
        
        color = material.emission;
        
        // PHONG
        if (lightningModel == 0) {
            float attenuation, Ndot, RdotVpow;
        
            // POINT LIGHT
            if (enableLights[0] != 0) {
                attenuation = 1.0 / (pointLight.attenuation[0] + pointLight.attenuation[1] * Vert.pointDistance + pointLight.attenuation[2] * Vert.pointDistance * Vert.pointDistance);

                color += material.ambient * pointLight.ambient * attenuation;

                Ndot = max(dot(normal, pointLightDir), 0.0);
                color += material.diffuse * pointLight.diffuse * Ndot * attenuation;

                RdotVpow = max(pow(dot(reflect(-pointLightDir, normal), viewDir), material.shininess), 0.0);
                color += material.specular * pointLight.specular * RdotVpow * attenuation;
            }
        
        
            // DIRECTIONAL LIGHT
            if (enableLights[1] != 0) {
                color += material.ambient * dirLight.ambient;

                Ndot = max(dot(normal, dirLightDir), 0.0);
                color += material.diffuse * dirLight.diffuse * Ndot;

                RdotVpow = max(pow(dot(reflect(-dirLightDir, normal), viewDir), material.shininess), 0.0);
                color += material.specular * dirLight.specular * RdotVpow;
            }
        

            // SPOTLIGHT
            if (enableLights[2] != 0) {
                float spotEffect = dot(normalize(spotLight.spotDirection),-spotLightDir);
                float spot = float(spotEffect > spotLight.spotCutoff);
                spotEffect = max(pow(spotEffect, spotLight.spotExponent), 0.0);

                attenuation = spot * spotEffect / (spotLight.attenuation[0] + spotLight.attenuation[1] * Vert.spotDistance + spotLight.attenuation[2] * Vert.spotDistance * Vert.spotDistance);

                color += material.ambient * spotLight.ambient * attenuation;

                Ndot = max(dot(normal, spotLightDir), 0.0);
                color += material.diffuse * spotLight.diffuse * Ndot * attenuation;

                RdotVpow = max(pow(dot(reflect(-spotLightDir, normal), viewDir), material.shininess), 0.0);
                color += material.specular * spotLight.specular * RdotVpow * attenuation;
            }
        }
        
        // TOON SHADING
        if (lightningModel == 1) {
            float attenuation, Ndot, RdotVpow;

            // POINT LIGHT
            if (enableLights[0] != 0) {
                color += material.ambient * pointLight.ambient;
                Ndot = max(dot(normal, pointLightDir), 0.0);
                
                float coef = 0.0f;
                if (Ndot < 0.4f)
                    coef = 0.3;
                else if (Ndot < 0.7f)
                    coef = 1;
                else coef = 1.3f;

                color += material.diffuse * pointLight.diffuse * coef;
            }
        
        
            // DIRECTIONAL LIGHT
            if (enableLights[1] != 0) {
                color += material.ambient * dirLight.ambient;
                Ndot = max(dot(normal, dirLightDir), 0.0);

                float coef = 0.0f;
                if (Ndot < 0.4f)
                    coef = 0.3;
                else if (Ndot < 0.7f)
                    coef = 1;
                else coef = 1.3f;

                color += material.diffuse * dirLight.diffuse * coef;
            }
        

            // SPOTLIGHT
            if (enableLights[2] != 0) {
                float spotEffect = dot(normalize(spotLight.spotDirection),-spotLightDir);
                float spot = float(spotEffect > spotLight.spotCutoff);
                spotEffect = max(pow(spotEffect, spotLight.spotExponent), 0.0);

                attenuation = spot * spotEffect / (spotLight.attenuation[0] + spotLight.attenuation[1] * Vert.spotDistance + spotLight.attenuation[2] * Vert.spotDistance * Vert.spotDistance);

                color += material.ambient * spotLight.ambient * attenuation;

                Ndot = max(dot(normal, spotLightDir), 0.0) * attenuation;

                float coef = 0.0f;
                if (Ndot < 0.4f)
                    coef = 0.3;
                else if (Ndot < 0.7f)
                    coef = 1;
                else coef = 1.3f;

                color += material.diffuse * spotLight.diffuse * coef;

            }
        }

        // RIM
        if (lightningModel == 2) {
            float attenuation, Ndot, RdotVpow;
        
            // POINT LIGHT
            if (enableLights[0] != 0) {
                attenuation = 1.0 / (pointLight.attenuation[0] + pointLight.attenuation[1] * Vert.pointDistance + pointLight.attenuation[2] * Vert.pointDistance * Vert.pointDistance);

                color += material.ambient * pointLight.ambient * attenuation;

                Ndot = max(dot(normal, pointLightDir), 0.0);
                color += material.diffuse * pointLight.diffuse * Ndot * attenuation;

                RdotVpow = max(pow(dot(reflect(-pointLightDir, normal), viewDir), material.shininess), 0.0);
                color += material.specular * pointLight.specular * RdotVpow * attenuation;

                float rim = pow(1.0 + 0.3 - max(dot(normal, viewDir), 0.0), 8.0);
                color += vec4(0.5, 0.0, 0.2, 1.0) * rim;
            }
        
        
            // DIRECTIONAL LIGHT
            if (enableLights[1] != 0) {
                color += material.ambient * dirLight.ambient;

                Ndot = max(dot(normal, dirLightDir), 0.0);
                color += material.diffuse * dirLight.diffuse * Ndot;

                RdotVpow = max(pow(dot(reflect(-dirLightDir, normal), viewDir), material.shininess), 0.0);
                color += material.specular * dirLight.specular * RdotVpow;

                float rim = pow(1.0 + 0.3 - max(dot(normal, viewDir), 0.0), 8.0);
                color += vec4(0.5, 0.0, 0.2, 1.0) * rim;
            }
        

            // SPOTLIGHT
            if (enableLights[2] != 0) {
                float spotEffect = dot(normalize(spotLight.spotDirection),-spotLightDir);
                float spot = float(spotEffect > spotLight.spotCutoff);
                spotEffect = max(pow(spotEffect, spotLight.spotExponent), 0.0);

                attenuation = spot * spotEffect / (spotLight.attenuation[0] + spotLight.attenuation[1] * Vert.spotDistance + spotLight.attenuation[2] * Vert.spotDistance * Vert.spotDistance);

                color += material.ambient * spotLight.ambient * attenuation;

                Ndot = max(dot(normal, spotLightDir), 0.0);
                color += material.diffuse * spotLight.diffuse * Ndot * attenuation;

                RdotVpow = max(pow(dot(reflect(-spotLightDir, normal), viewDir), material.shininess), 0.0);
                color += material.specular * spotLight.specular * RdotVpow * attenuation;

                float rim = pow(1.0 + 0.3 - max(dot(normal, viewDir), 0.0), 8.0);
                color += vec4(0.5, 0.0, 0.2, 1.0) * rim;
            }
        }

        // BIDIRECTIONAL LIGHTNING
        if (lightningModel == 3) {
            vec4 color2 = vec4(0.5f,0.5f, 0.0f, 1.0f);
            float attenuation, Ndot, RdotVpow;
            // POINT LIGHT
            if (enableLights[0] != 0) {
                color += material.ambient * pointLight.ambient;

                color += material.diffuse * (pointLight.diffuse * max(dot(normal, pointLightDir), 0.0) + color2 * max(dot(normal, -pointLightDir), 0.0));
            }
        
        
            // DIRECTIONAL LIGHT
            if (enableLights[1] != 0) {
                color += material.ambient * dirLight.ambient;

                color += material.diffuse * (dirLight.diffuse * max(dot(normal, dirLightDir), 0.0) + color2 * max(dot(normal, -dirLightDir), 0.0));
            }
        

            // SPOTLIGHT
            if (enableLights[2] != 0) {
                float spotEffect = dot(normalize(spotLight.spotDirection),-spotLightDir);
                float spot = float(spotEffect > spotLight.spotCutoff);
                spotEffect = max(pow(spotEffect, spotLight.spotExponent), 0.0);

                attenuation = spot * spotEffect / (spotLight.attenuation[0] + spotLight.attenuation[1] * Vert.spotDistance + spotLight.attenuation[2] * Vert.spotDistance * Vert.spotDistance);

                color += material.ambient * spotLight.ambient * attenuation;

                color += material.diffuse * (spotLight.diffuse * max(dot(normal, spotLightDir), 0.0) + color2 * max(dot(normal, -spotLightDir), 0.0)) * attenuation;
            }
        }

        // TEXTURE
        color *= texture(material.texture, Vert.texcoord);
    }
)";


time_t start_time;

vector<Vertex_Texture_Normal> obj0;
vector<Vertex_Texture_Normal> obj1;
vector<Vertex_Texture_Normal> obj2;
vector<Vertex_Texture_Normal> obj3;
vector<Vertex_Texture_Normal> obj4;

void checkOpenGLerror() {
    GLenum err;
    while ((err = glGetError()) != GL_NO_ERROR)
    {
        cout << "Error! Code: " << hex << err << dec << endl;
    }
}

void ShaderLog(unsigned int shader)
{
    int infologLen = 0;
    glGetShaderiv(shader, GL_INFO_LOG_LENGTH, &infologLen);
    if (infologLen > 1)
    {
        int charsWritten = 0;
        vector<char> infoLog(infologLen);
        glGetShaderInfoLog(shader, infologLen, &charsWritten, infoLog.data());
        cout << "InfoLog: " << infoLog.data() << endl;
    }
}

obj read_obj(string filename) {
    ifstream f(filename);

    vector<string> verteces;
    vector<string> textureCoords;
    vector<string> polygons;
    vector<string> normals;

    string s;

    while (getline(f, s)) {
        if (s != "") {
            if (s[0] == 'f') polygons.push_back(s);

            if (s[0] == 'v') {
                if (s[1] == ' ') verteces.push_back(s);
                if (s[1] == 't') textureCoords.push_back(s);
                if (s[1] == 'n') normals.push_back(s);
            }
        }
    }

    obj o;

    for (int i = 0; i < verteces.size(); i++) {
        string parsed, input = verteces[i];
        stringstream input_stringstream(input);

        Vertex v;

        getline(input_stringstream, parsed, ' ');
        getline(input_stringstream, parsed, ' ');
        while (parsed == "") getline(input_stringstream, parsed, ' ');

        v.x = atof(parsed.c_str());
        getline(input_stringstream, parsed, ' ');
        v.y = atof(parsed.c_str());
        getline(input_stringstream, parsed, ' ');
        v.z = atof(parsed.c_str());

        o.verteces.push_back(v);
    }

    for (int i = 0; i < textureCoords.size(); i++) {
        string parsed, input = textureCoords[i];
        stringstream input_stringstream(input);

        TextureCoordinate t;

        getline(input_stringstream, parsed, ' ');
        getline(input_stringstream, parsed, ' ');
        while (parsed == "") getline(input_stringstream, parsed, ' ');

        t.x = atof(parsed.c_str());
        getline(input_stringstream, parsed, ' ');
        t.y = atof(parsed.c_str());

        o.textureCoords.push_back(t);
    }

    for (int i = 0; i < polygons.size(); i++) {
        string parsed, input = polygons[i];
        stringstream input_stringstream(input);

        vector<PolygonIndices> polygon;

        getline(input_stringstream, parsed, ' ');

        o.polygons.push_back(vector<PolygonIndices>());

        while (getline(input_stringstream, parsed, ' ')) {
            if (parsed == "") continue;
            string ind, indices = parsed;
            stringstream index_stringstream(indices);

            PolygonIndices pind;
            getline(index_stringstream, ind, '/');
            pind.vertexIndex = atoi(ind.c_str());
            getline(index_stringstream, ind, '/');
            pind.textureCoordIndex = atoi(ind.c_str());
            getline(index_stringstream, ind, '/');
            pind.normalIndex = atoi(ind.c_str());

            o.polygons.back().push_back(pind);
        }
    }

    for (int i = 0; i < normals.size(); i++) {
        string parsed, input = normals[i];
        stringstream input_stringstream(input);

        Normal n;

        getline(input_stringstream, parsed, ' ');
        getline(input_stringstream, parsed, ' ');
        while (parsed == "") getline(input_stringstream, parsed, ' ');

        n.x = atof(parsed.c_str());
        getline(input_stringstream, parsed, ' ');
        n.y = atof(parsed.c_str());
        getline(input_stringstream, parsed, ' ');
        n.z = atof(parsed.c_str());

        o.normals.push_back(n);
    }

    f.close();
    return o;
}

void triangulate(obj& o) {
    vector<vector<PolygonIndices>> polygons;

    for (int i = 0; i < o.polygons.size(); i++) {
        if (o.polygons[i].size() == 3) {
            polygons.push_back(o.polygons[i]);
            continue;
        }
        for (int j = 2; j < o.polygons[i].size(); j++) {
            vector<PolygonIndices> polygon;

            polygon.push_back(o.polygons[i][0]);
            polygon.push_back(o.polygons[i][j - 1]);
            polygon.push_back(o.polygons[i][j]);

            polygons.push_back(polygon);
        }
    }

    o.polygons = polygons;
}

vector<Vertex_Texture_Normal> obj_to_buffer_data(obj o) {
    vector<Vertex_Texture_Normal> res;

    for (int i = 0; i < o.polygons.size(); i++) {
        for (int j = 0; j < o.polygons[i].size(); j++) {
            Vertex_Texture_Normal vtn;
            vtn.v = o.verteces[o.polygons[i][j].vertexIndex - 1];
            vtn.t = o.textureCoords[o.polygons[i][j].textureCoordIndex - 1];
            vtn.n = o.normals[o.polygons[i][j].normalIndex - 1];
            res.push_back(vtn);
        }
    }

    return res;
}

void InitVBO() {
    glGenBuffers(1, &VBO0);
    glGenBuffers(1, &VBO1);
    glGenBuffers(1, &VBO2);
    glGenBuffers(1, &VBO3);
    glGenBuffers(1, &VBO4);

    //__________________________________________
    
    obj o = read_obj("table.obj");
    triangulate(o);
    obj0 = obj_to_buffer_data(o);

    // Передаем вершины в буфер
    glBindBuffer(GL_ARRAY_BUFFER, VBO0);
    glBufferData(GL_ARRAY_BUFFER, sizeof(GLfloat) * 8 * obj0.size(), obj0.data(), GL_STATIC_DRAW);
    glBindBuffer(GL_ARRAY_BUFFER, 0);
    checkOpenGLerror(); //Пример функции есть в лабораторной

    //__________________________________________

    o = read_obj("bag.obj");
    triangulate(o);
    obj1 = obj_to_buffer_data(o);

    glBindBuffer(GL_ARRAY_BUFFER, VBO1);
    glBufferData(GL_ARRAY_BUFFER, sizeof(GLfloat) * 8 * obj1.size(), obj1.data(), GL_STATIC_DRAW);
    glBindBuffer(GL_ARRAY_BUFFER, 0);
    checkOpenGLerror(); //Пример функции есть в лабораторной

    //__________________________________________

    o = read_obj("map.obj");
    triangulate(o);
    obj2 = obj_to_buffer_data(o);

    glBindBuffer(GL_ARRAY_BUFFER, VBO2);
    glBufferData(GL_ARRAY_BUFFER, sizeof(GLfloat) * 8 * obj2.size(), obj2.data(), GL_STATIC_DRAW);
    glBindBuffer(GL_ARRAY_BUFFER, 0);
    checkOpenGLerror(); //Пример функции есть в лабораторной

    //__________________________________________

    o = read_obj("skull.obj");
    triangulate(o);
    obj3 = obj_to_buffer_data(o);

    glBindBuffer(GL_ARRAY_BUFFER, VBO3);
    glBufferData(GL_ARRAY_BUFFER, sizeof(GLfloat) * 8 * obj3.size(), obj3.data(), GL_STATIC_DRAW);
    glBindBuffer(GL_ARRAY_BUFFER, 0);
    checkOpenGLerror(); //Пример функции есть в лабораторной

    //__________________________________________

    o = read_obj("bottle.obj");
    triangulate(o);
    obj4 = obj_to_buffer_data(o);

    glBindBuffer(GL_ARRAY_BUFFER, VBO4);
    glBufferData(GL_ARRAY_BUFFER, sizeof(GLfloat) * 8 * obj4.size(), obj4.data(), GL_STATIC_DRAW);
    glBindBuffer(GL_ARRAY_BUFFER, 0);
    checkOpenGLerror(); //Пример функции есть в лабораторной
}

void InitShader() {
    GLuint vShader = glCreateShader(GL_VERTEX_SHADER);
    glShaderSource(vShader, 1, &VertexShaderSource, NULL);
    glCompileShader(vShader);
    cout << "vertex shader \n";
    ShaderLog(vShader); //Пример функции есть в лабораторной

    GLuint fShader = glCreateShader(GL_FRAGMENT_SHADER);
    glShaderSource(fShader, 1, &FragShaderSource, NULL);
    glCompileShader(fShader);
    cout << "fragment shader \n";
    ShaderLog(fShader);

    Program = glCreateProgram();
    glAttachShader(Program, vShader);
    glAttachShader(Program, fShader);
    glLinkProgram(Program);

    int link_ok;
    glGetProgramiv(Program, GL_LINK_STATUS, &link_ok);
    if (!link_ok) {
        cout << "error attach shaders \n";
        return;
    }

    checkOpenGLerror();

    coordAttribID = glGetAttribLocation(Program, "position");
    textureCoordAttribID = glGetAttribLocation(Program, "texcoord");
    normalAttribID = glGetAttribLocation(Program, "normal");


    transformModelUniformID = glGetUniformLocation(Program, "transform.model");
    transformViewProjectionUniformID = glGetUniformLocation(Program, "transform.viewProjection");
    transformNormalUniformID = glGetUniformLocation(Program, "transform.normal");
    transformViewPositionUniformID = glGetUniformLocation(Program, "transform.viewPosition");

    materialTextureUniformID = glGetUniformLocation(Program, "material.Texture");
    materialAmbientUniformID = glGetUniformLocation(Program, "material.ambient");
    materialDiffuseUniformID = glGetUniformLocation(Program, "material.diffuse");
    materialSpecularUniformID = glGetUniformLocation(Program, "material.specular");
    materialEmissionUniformID = glGetUniformLocation(Program, "material.emission");
    materialShininessUniformID = glGetUniformLocation(Program, "material.shininess");

    enableLightsUniformID = glGetUniformLocation(Program, "enableLights");
    lightningModelUniformID = glGetUniformLocation(Program, "lightningModel");

    pointLightpositionUniformID = glGetUniformLocation(Program, "pointLight.position");
    pointLightAmbientUniformID = glGetUniformLocation(Program, "pointLight.ambient");
    pointLightDiffuseUniformID = glGetUniformLocation(Program, "pointLight.diffuse");
    pointLightSpecularUniformID = glGetUniformLocation(Program, "pointLight.specular");
    pointLightAttenuationUniformID = glGetUniformLocation(Program, "pointLight.attenuation");

    directionalLightDirectionUniformID = glGetUniformLocation(Program, "dirLight.direction");
    directionalLightAmbientUniformID = glGetUniformLocation(Program, "dirLight.ambient");
    directionalLightDiffuseUniformID = glGetUniformLocation(Program, "dirLight.diffuse");
    directionalLightSpecularUniformID = glGetUniformLocation(Program, "dirLight.specular");

    spotLightPositionUniformID = glGetUniformLocation(Program, "spotLight.position");
    spotLightAmbientUniformID = glGetUniformLocation(Program, "spotLight.ambient");
    spotLightDiffuseUniformID = glGetUniformLocation(Program, "spotLight.diffuse");
    spotLightSpecularUniformID = glGetUniformLocation(Program, "spotLight.specular");
    spotLightAttenuationUniformID = glGetUniformLocation(Program, "spotLight.attenuation");
    spotLightSpotDirectionUniformID = glGetUniformLocation(Program, "spotLight.spotDirection");
    spotLightSpotCutoffUniformID = glGetUniformLocation(Program, "spotLight.spotCutoff");
    spotLightSpotExponentUniformID = glGetUniformLocation(Program, "spotLight.spotExponent");

}

GLuint texture0;
GLuint texture1;
GLuint texture2;
GLuint texture3;
GLuint texture4;

void Init() {
    // Шейдеры
    InitShader();
    // Вершинный буфер
    InitVBO();

    glEnable(GL_DEPTH_TEST);
    
    int width, height;
    unsigned char* image;


    glGenTextures(1, &texture0);
    glBindTexture(GL_TEXTURE_2D, texture0);

    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

    image = SOIL_load_image("table.png", &width, &height, 0, SOIL_LOAD_RGB);
    cout << SOIL_last_result() << endl;
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, image);

    glGenerateMipmap(GL_TEXTURE_2D);
    SOIL_free_image_data(image);
    glBindTexture(GL_TEXTURE_2D, 0);
    
    //__________________________________________

    glGenTextures(1, &texture1);
    glBindTexture(GL_TEXTURE_2D, texture1);

    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

    image = SOIL_load_image("bag.png", &width, &height, 0, SOIL_LOAD_RGB);
    cout << SOIL_last_result() << endl;
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, image);

    glGenerateMipmap(GL_TEXTURE_2D);
    SOIL_free_image_data(image);
    glBindTexture(GL_TEXTURE_2D, 0);

    //__________________________________________

    glGenTextures(1, &texture2);
    glBindTexture(GL_TEXTURE_2D, texture2);

    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

    image = SOIL_load_image("map.png", &width, &height, 0, SOIL_LOAD_RGB);
    cout << SOIL_last_result() << endl;
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, image);

    glGenerateMipmap(GL_TEXTURE_2D);
    SOIL_free_image_data(image);
    glBindTexture(GL_TEXTURE_2D, 0);

    //__________________________________________

    glGenTextures(1, &texture3);
    glBindTexture(GL_TEXTURE_2D, texture3);

    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

    image = SOIL_load_image("skull.png", &width, &height, 0, SOIL_LOAD_RGB);
    cout << SOIL_last_result() << endl;
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, image);

    glGenerateMipmap(GL_TEXTURE_2D);
    SOIL_free_image_data(image);
    glBindTexture(GL_TEXTURE_2D, 0);

    //__________________________________________

    glGenTextures(1, &texture4);
    glBindTexture(GL_TEXTURE_2D, texture4);

    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

    image = SOIL_load_image("bottle.png", &width, &height, 0, SOIL_LOAD_RGB);
    cout << SOIL_last_result() << endl;
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, image);

    glGenerateMipmap(GL_TEXTURE_2D);
    SOIL_free_image_data(image);
    glBindTexture(GL_TEXTURE_2D, 0);

}

glm::vec4 cameraPos = glm::vec4(0.0f, 0.0f, 3.0f, 0.0f);
glm::vec4 cameraRot = glm::vec4(0.0f, 0.0f, 0.0f, 0.0f);

glm::vec4 cameraUp = glm::vec4(0.0f, 1.0f, 0.0f, 0.0f);
glm::vec4 cameraRight = glm::vec4(1.0f, 0.0f, 0.0f, 0.0f);
glm::vec4 cameraFront = glm::vec4(0.0f, 0.0f, -1.0f, 0.0f);

glm::mat4 CameraRotation(glm::vec3 rot) {
    glm::mat4 res = glm::mat4(1.0f);
    res = glm::rotate(res, (GLfloat)-rot.y, glm::vec3(0.0f, 1.0f, 0.0f));
    res = glm::rotate(res, (GLfloat)-rot.x, glm::vec3(1.0f, 0.0f, 0.0f));
    return res;
}

GLfloat cameraSpeed = 0.02;
glm::vec3 enableLights = {0, 0, 0};
int lightningModel = 0;

void Draw() {
    glUseProgram(Program); // Устанавливаем шейдерную программу текущей

    glm::mat4 projection = glm::perspective(45.0f, (GLfloat)1000 / (GLfloat)1000, 0.1f, 100.0f);
    glm::mat4 view = glm::mat4(1.0f);
    view = CameraRotation(cameraRot) * glm::translate(view, -glm::vec3(cameraPos));

    glm::mat4 viewProjection = projection * view;

    glUniform3f(transformViewPositionUniformID, cameraPos.x, cameraPos.y, cameraPos.z);
    glUniformMatrix4fv(transformViewProjectionUniformID, 1, GL_FALSE, glm::value_ptr(viewProjection));

    glUniform3f(enableLightsUniformID, enableLights[0], enableLights[1], enableLights[2]);
    glUniform1i(lightningModelUniformID, lightningModel);

    glUniform4f(pointLightpositionUniformID, 0, 0, 0.7f, 0);
    glUniform4f(pointLightAmbientUniformID, 0.1f, 0.1f, 0.1f, 0);
    glUniform4f(pointLightDiffuseUniformID, 0.8f, 0.8f, 0.8f, 0);
    glUniform4f(pointLightSpecularUniformID, 0.3f, 0.3f, 0.3f, 0);
    glUniform3f(pointLightAttenuationUniformID, 1, 0, 0);

    glUniform4f(directionalLightDirectionUniformID, 0, -1, 1, 0);
    glUniform4f(directionalLightAmbientUniformID, 0.1f, 0.1f, 0.1f, 0);
    glUniform4f(directionalLightDiffuseUniformID, 0.8f, 0.8f, 0.8f, 0);
    glUniform4f(directionalLightSpecularUniformID, 0.3f, 0.3f, 0.3f, 0);

    glUniform4f(spotLightPositionUniformID, 0, 0, 1, 0);
    glUniform4f(spotLightAmbientUniformID, 0.1f, 0.1f, 0.1f, 0);
    glUniform4f(spotLightDiffuseUniformID, 0.8f, 0.8f, 0.8f, 0);
    glUniform4f(spotLightSpecularUniformID, 0.3f, 0.3f, 0.3f, 0);
    glUniform3f(spotLightAttenuationUniformID, 1, 0, 0);
    glUniform3f(spotLightSpotDirectionUniformID, 2.0f, 2.0f, -1);
    glUniform1f(spotLightSpotCutoffUniformID, 0);
    glUniform1f(spotLightSpotExponentUniformID, 1);


    glEnableVertexAttribArray(coordAttribID);
    glEnableVertexAttribArray(textureCoordAttribID);
    glEnableVertexAttribArray(normalAttribID);

    //___________________________________________________________________________
    // TABLE

    glm::mat4 model0 = glm::mat4(1.0f);
    model0 = glm::scale(model0, glm::vec3(0.02f, 0.02f, 0.02f));
    model0 = glm::rotate(model0, (GLfloat)glm::radians(90.0f), glm::vec3(1.0f, 0.0f, 0.0f));

    glm::mat3 normal0 = glm::mat3(1.0f);
    normal0 = glm::rotate(glm::mat4(normal0), glm::radians(90.0f), glm::vec3(1.0f, 0.0f, 0.0f));
    

    glUniformMatrix4fv(transformModelUniformID, 1, GL_FALSE, glm::value_ptr(model0));
    glUniformMatrix3fv(transformNormalUniformID, 1, GL_FALSE, glm::value_ptr(normal0));

    glUniform4f(materialAmbientUniformID, 1, 1, 1, 1);
    glUniform4f(materialDiffuseUniformID, 1, 1, 1, 1);
    glUniform4f(materialSpecularUniformID, 1, 1, 1, 1);
    glUniform4f(materialEmissionUniformID, 0.0f, 0.0f, 0.0f, 0);
    glUniform1f(materialShininessUniformID, 1.0f);

    glActiveTexture(GL_TEXTURE0);
    glBindTexture(GL_TEXTURE_2D, texture0);
    glUniform1i(materialTextureUniformID, 0);

    glBindBuffer(GL_ARRAY_BUFFER, VBO0);
    glVertexAttribPointer(coordAttribID, 3, GL_FLOAT, GL_FALSE, 32, (GLvoid*)0);
    glVertexAttribPointer(textureCoordAttribID, 2, GL_FLOAT, GL_FALSE, 32, (GLvoid*)12);
    glVertexAttribPointer(normalAttribID, 3, GL_FLOAT, GL_FALSE, 32, (GLvoid*)20);
    glBindBuffer(GL_ARRAY_BUFFER, 0); // Отключаем VBO

    glDrawArrays(GL_TRIANGLES, 0, obj0.size()); // Передаем данные на видеокарту(рисуем)

    glBindTexture(GL_TEXTURE_2D, 0);

    //___________________________________________________________________________
    // BAG

    glm::mat4 model1 = glm::mat4(1.0f);
    model1 = glm::translate(model1, glm::vec3(0.35f, 0.25f, 0.5f));
    model1 = glm::scale(model1, glm::vec3(0.01f, 0.01f, 0.01f));
    
    glm::mat3 normal1 = glm::mat3(1.0f);

    glUniformMatrix4fv(transformModelUniformID, 1, GL_FALSE, glm::value_ptr(model1));
    glUniformMatrix3fv(transformNormalUniformID, 1, GL_FALSE, glm::value_ptr(normal1));

    glUniform4f(materialAmbientUniformID, 1, 1, 1, 1);
    glUniform4f(materialDiffuseUniformID, 1, 1, 1, 1);
    glUniform4f(materialSpecularUniformID, 1, 1, 1, 1);
    glUniform4f(materialEmissionUniformID, 0.0f, 0.0f, 0.0f, 0);
    glUniform1f(materialShininessUniformID, 1.0f);

    glBindTexture(GL_TEXTURE_2D, texture1);
    glUniform1i(materialTextureUniformID, 1);

    glBindBuffer(GL_ARRAY_BUFFER, VBO1);
    glVertexAttribPointer(coordAttribID, 3, GL_FLOAT, GL_FALSE, 32, (GLvoid*)0);
    glVertexAttribPointer(textureCoordAttribID, 2, GL_FLOAT, GL_FALSE, 32, (GLvoid*)12);
    glVertexAttribPointer(normalAttribID, 3, GL_FLOAT, GL_FALSE, 32, (GLvoid*)20);
    glBindBuffer(GL_ARRAY_BUFFER, 0); // Отключаем VBO

    glDrawArrays(GL_TRIANGLES, 0, obj1.size()); // Передаем данные на видеокарту(рисуем)

    glBindTexture(GL_TEXTURE_2D, 0);

    //___________________________________________________________________________
    // MAP
    
    glm::mat4 model2 = glm::mat4(1.0f);
    model2 = glm::translate(model2, glm::vec3(0.0f, 0.0f, 0.5f));
    model2 = glm::scale(model2, glm::vec3(0.009f, 0.009f, 0.009f));

    glm::mat3 normal2 = glm::mat3(1.0f);

    glUniformMatrix4fv(transformModelUniformID, 1, GL_FALSE, glm::value_ptr(model2));
    glUniformMatrix3fv(transformNormalUniformID, 1, GL_FALSE, glm::value_ptr(normal2));

    glUniform4f(materialAmbientUniformID, 1, 1, 1, 1);
    glUniform4f(materialDiffuseUniformID, 1, 1, 1, 1);
    glUniform4f(materialSpecularUniformID, 1, 1, 1, 1);
    glUniform4f(materialEmissionUniformID, 0.0f, 0.0f, 0.0f, 0);
    glUniform1f(materialShininessUniformID, 1.0f);

    glBindTexture(GL_TEXTURE_2D, texture2);
    glUniform1i(materialTextureUniformID, 2);

    glBindBuffer(GL_ARRAY_BUFFER, VBO2);
    glVertexAttribPointer(coordAttribID, 3, GL_FLOAT, GL_FALSE, 32, (GLvoid*)0);
    glVertexAttribPointer(textureCoordAttribID, 2, GL_FLOAT, GL_FALSE, 32, (GLvoid*)12);
    glVertexAttribPointer(normalAttribID, 3, GL_FLOAT, GL_FALSE, 32, (GLvoid*)20);
    glBindBuffer(GL_ARRAY_BUFFER, 0); // Отключаем VBO

    glDrawArrays(GL_TRIANGLES, 0, obj2.size()); // Передаем данные на видеокарту(рисуем)

    glBindTexture(GL_TEXTURE_2D, 0);

    //___________________________________________________________________________
    // SKULL
    
    glm::mat4 model3 = glm::mat4(1.0f);
    model3 = glm::translate(model3, glm::vec3(cameraPos));
    model3 = glm::translate(model3, glm::vec3(0.0f, 0.5f, -1.5f));
    model3 = glm::scale(model3, glm::vec3(0.008f, 0.008f, 0.008f));
    model3 = glm::rotate(model3, glm::radians(0.0f), glm::vec3(0.0f, 0.0f, 1.0f));
    
    glm::mat3 normal3 = glm::mat3(1.0f);
    normal3 = glm::rotate(glm::mat4(normal3), glm::radians(30.0f), glm::vec3(0.0f, 0.0f, 1.0f));

    glUniformMatrix4fv(transformModelUniformID, 1, GL_FALSE, glm::value_ptr(model3));
    glUniformMatrix3fv(transformNormalUniformID, 1, GL_FALSE, glm::value_ptr(normal3));

    glUniform4f(materialAmbientUniformID, 1, 1, 1, 1);
    glUniform4f(materialDiffuseUniformID, 1, 1, 1, 1);
    glUniform4f(materialSpecularUniformID, 1, 1, 1, 1);
    glUniform4f(materialEmissionUniformID, 0.0f, 0.0f, 0.0f, 0);
    glUniform1f(materialShininessUniformID, 1.0f);

    glBindTexture(GL_TEXTURE_2D, texture3);
    glUniform1i(materialTextureUniformID, 3);

    glBindBuffer(GL_ARRAY_BUFFER, VBO3);
    glVertexAttribPointer(coordAttribID, 3, GL_FLOAT, GL_FALSE, 32, (GLvoid*)0);
    glVertexAttribPointer(textureCoordAttribID, 2, GL_FLOAT, GL_FALSE, 32, (GLvoid*)12);
    glVertexAttribPointer(normalAttribID, 3, GL_FLOAT, GL_FALSE, 32, (GLvoid*)20);
    glBindBuffer(GL_ARRAY_BUFFER, 0); // Отключаем VBO

    glDrawArrays(GL_TRIANGLES, 0, obj3.size()); // Передаем данные на видеокарту(рисуем)

    glBindTexture(GL_TEXTURE_2D, 0);

    //___________________________________________________________________________
    // BOTTLE

    glm::mat4 model4 = glm::mat4(1.0f);
    model4 = glm::translate(model4, glm::vec3(0.55f, 0.0f, 0.5f));
    model4 = glm::scale(model4, glm::vec3(0.03f, 0.03f, 0.03f));
    model4 = glm::rotate(model4, glm::radians(-120.0f), glm::vec3(0.0f, 0.0f, 1.0f));

    glm::mat3 normal4 = glm::mat3(1.0f);
    normal4 = glm::rotate(glm::mat4(normal4), glm::radians(-120.0f), glm::vec3(0.0f, 0.0f, 1.0f));

    glUniformMatrix4fv(transformModelUniformID, 1, GL_FALSE, glm::value_ptr(model4));
    glUniformMatrix3fv(transformNormalUniformID, 1, GL_FALSE, glm::value_ptr(normal4));

    glUniform4f(materialAmbientUniformID, 1, 1, 1, 1);
    glUniform4f(materialDiffuseUniformID, 1, 1, 1, 1);
    glUniform4f(materialSpecularUniformID, 1, 1, 1, 1);
    glUniform4f(materialEmissionUniformID, 0.0f, 0.0f, 0.0f, 0);
    glUniform1f(materialShininessUniformID, 1.0f);

    glBindTexture(GL_TEXTURE_2D, texture4);
    glUniform1i(materialTextureUniformID, 4);

    glBindBuffer(GL_ARRAY_BUFFER, VBO4);
    glVertexAttribPointer(coordAttribID, 3, GL_FLOAT, GL_FALSE, 32, (GLvoid*)0);
    glVertexAttribPointer(textureCoordAttribID, 2, GL_FLOAT, GL_FALSE, 32, (GLvoid*)12);
    glVertexAttribPointer(normalAttribID, 3, GL_FLOAT, GL_FALSE, 32, (GLvoid*)20);
    glBindBuffer(GL_ARRAY_BUFFER, 0); // Отключаем VBO

    glDrawArrays(GL_TRIANGLES, 0, obj4.size()); // Передаем данные на видеокарту(рисуем)

    glBindTexture(GL_TEXTURE_2D, 0);

    //___________________________________________________________________________

    glDisableVertexAttribArray(coordAttribID); // Отключаем массив атрибутов
    glDisableVertexAttribArray(textureCoordAttribID); // Отключаем массив атрибутов
    glDisableVertexAttribArray(normalAttribID); // Отключаем массив атрибутов

    glUseProgram(0); // Отключаем шейдерную программу

    checkOpenGLerror();
}

// Освобождение буфера
void ReleaseVBO() {
    glBindBuffer(GL_ARRAY_BUFFER, 0);
    glDeleteBuffers(1, &VBO0);
    glDeleteBuffers(1, &VBO1);
}

// Освобождение шейдеров
void ReleaseShader() {
    // Передавая ноль, мы отключаем шейдерную программу
    glUseProgram(0);
    // Удаляем шейдерную программу
    glDeleteProgram(Program);
}

void Release() {
    // Шейдеры
    ReleaseShader();
    // Вершинный буфер
    ReleaseVBO();
}

int main() {
    sf::Window window(sf::VideoMode(1000, 1000), "My OpenGL window", sf::Style::Default, sf::ContextSettings(24));
    window.setVerticalSyncEnabled(true);
    window.setActive(true);
    glewInit();
    Init();
    start_time = clock();
    //cameraRight = glm::normalize(glm::cross(cameraFront, WorldUp));
    while (window.isOpen()) {
        
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Num1)) {
            if (enableLights[0] == 0.0) enableLights[0] = 1.0;
            else enableLights[0] = 0.0;
            while (sf::Keyboard::isKeyPressed(sf::Keyboard::Num1)) {}
        }
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Num2)) {
            if (enableLights[1] == 0.0) enableLights[1] = 1.0;
            else enableLights[1] = 0.0;
            while (sf::Keyboard::isKeyPressed(sf::Keyboard::Num2)) {}
        }
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Num3)) {
            if (enableLights[2] == 0.0) enableLights[2] = 1.0;
            else enableLights[2] = 0.0;
            while (sf::Keyboard::isKeyPressed(sf::Keyboard::Num3)) {}
        }


        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Num4)) {
            lightningModel = 0;
            while (sf::Keyboard::isKeyPressed(sf::Keyboard::Num4)) {}
        }
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Num5)) {
            lightningModel = 1;
            while (sf::Keyboard::isKeyPressed(sf::Keyboard::Num5)) {}
        }
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Num6)) {
            lightningModel = 2;
            while (sf::Keyboard::isKeyPressed(sf::Keyboard::Num6)) {}
        }
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Num7)) {
            lightningModel = 3;
            while (sf::Keyboard::isKeyPressed(sf::Keyboard::Num7)) {}
        }

       /*if (sf::Keyboard::isKeyPressed(sf::Keyboard::Right)) {
            cameraRot.y -= cameraSpeed;
        }
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Left)) {
            cameraRot.y += cameraSpeed;
        }

        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Up)) {
            cameraRot.x += cameraSpeed;
        }
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Down)) {
            cameraRot.x -= cameraSpeed;
        }*/

        if (sf::Keyboard::isKeyPressed(sf::Keyboard::W)) 
            cameraPos += cameraUp * CameraRotation(cameraRot) * cameraSpeed;
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::S)) 
            cameraPos -= cameraUp * CameraRotation(cameraRot) * cameraSpeed;
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::D)) 
            cameraPos += cameraRight * CameraRotation(cameraRot) * cameraSpeed;
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::A)) 
            cameraPos -= cameraRight * CameraRotation(cameraRot) * cameraSpeed;
     
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::E)) 
            cameraPos += cameraFront * CameraRotation(cameraRot) * cameraSpeed;
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Q)) 
            cameraPos -= cameraFront * CameraRotation(cameraRot) * cameraSpeed;

        sf::Event event;
        while (window.pollEvent(event)) {
            if (event.type == sf::Event::Closed) { window.close(); break; }
            else if (event.type == sf::Event::Resized) { glViewport(0, 0, event.size.width, event.size.height); }
        }
        if (!window.isOpen()) continue;
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        Draw();
        window.display();
    }
    Release();


    return 0;
}
