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
GLint textureUniformID;
// ID Vertex Buffer Object
GLuint VBO;
GLuint VBO_map;

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

struct Vertex_Texture {
    Vertex v;
    TextureCoordinate t;
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
};

const GLfloat Pi = 3.14159274101257324219f;

// Исходный код вершинного шейдера
const char* VertexShaderSource = R"(
 #version 330 core
 in vec3 coord;
 in vec2 texCoord;
 out vec2 TexCoord; 

 uniform mat4 transform[5];
 uniform mat4 view;
 uniform mat4 projection;

 void main() {
    gl_Position = projection * view * transform[gl_InstanceID] * vec4(coord, 1.0f);
    TexCoord = vec2(texCoord.x, 1.0f - texCoord.y);
 }
)";

// Исходный код фрагментного шейдера
const char* FragShaderSource = R"(
 #version 330 core
 in vec2 TexCoord;

 out vec4 color;
 
 uniform sampler2D ourTexture;

 void main() {
    color = texture(ourTexture, TexCoord);
 }
)";

time_t start_time;

vector<Vertex_Texture> obj1;
vector<Vertex_Texture> obj2;

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

    string s;

    while (getline(f, s)) {
        if (s != "") {
            if (s[0] == 'f') polygons.push_back(s);

            if (s[0] == 'v') {
                if (s[1] == ' ') verteces.push_back(s);
                if (s[1] == 't') textureCoords.push_back(s);
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

vector<Vertex_Texture> obj_to_buffer_data(obj o) {
    vector<Vertex_Texture> res;

    for (int i = 0; i < o.polygons.size(); i++) {
        for (int j = 0; j < o.polygons[i].size(); j++) {
            Vertex_Texture vt;
            vt.v = o.verteces[o.polygons[i][j].vertexIndex - 1];
            vt.t = o.textureCoords[o.polygons[i][j].textureCoordIndex - 1];
            res.push_back(vt);
        }
    }

    return res;
}

void InitVBO() {
    glGenBuffers(1, &VBO);
    glGenBuffers(1, &VBO_map);
    
    obj o = read_obj("bag.obj");
    triangulate(o);
    obj1 = obj_to_buffer_data(o);

    // Передаем вершины в буфер
    glBindBuffer(GL_ARRAY_BUFFER, VBO);
    glBufferData(GL_ARRAY_BUFFER, sizeof(GLfloat) * 5 * obj1.size(), obj1.data(), GL_STATIC_DRAW);
    glBindBuffer(GL_ARRAY_BUFFER, 0);
    checkOpenGLerror(); //Пример функции есть в лабораторной

    o = read_obj("skull.obj");
    triangulate(o);
    obj2 = obj_to_buffer_data(o);

    glBindBuffer(GL_ARRAY_BUFFER, VBO_map);
    glBufferData(GL_ARRAY_BUFFER, sizeof(GLfloat) * 5 * obj2.size(), obj2.data(), GL_STATIC_DRAW);
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
    // Вытягиваем ID атрибута из собранной программы
    coordAttribID = glGetAttribLocation(Program, "coord");
    textureCoordAttribID = glGetAttribLocation(Program, "texCoord");

    textureUniformID = glGetUniformLocation(Program, "ourTexture");

    checkOpenGLerror();
}

GLuint texture1;
GLuint texture2;

void Init() {
    // Шейдеры
    InitShader();
    // Вершинный буфер
    InitVBO();

    glEnable(GL_DEPTH_TEST);
    
    glGenTextures(1, &texture1);
    glBindTexture(GL_TEXTURE_2D, texture1);

    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

    int width, height;
    unsigned char* image = SOIL_load_image("bag.png", &width, &height, 0, SOIL_LOAD_RGB);
    cout << SOIL_last_result() << endl;
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, image);

    glGenerateMipmap(GL_TEXTURE_2D);
    SOIL_free_image_data(image);
    glBindTexture(GL_TEXTURE_2D, 0);
    
    glGenTextures(1, &texture2);
    glBindTexture(GL_TEXTURE_2D, texture2);

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

}

glm::vec3 cameraPos = glm::vec3(0.0f, 0.0f, 3.0f);
glm::vec3 cameraFront = glm::vec3(0.0f, 0.0f, -1.0f);
glm::vec3 cameraUp = glm::vec3(0.0f, 1.0f, 0.0f);
glm::vec3 cameraRight = glm::vec3(1.0f, 0.0f, 0.0f);

//glm::vec3 WorldUp = glm::vec3(0.0f, 1.0f, 0.0f);

GLfloat cameraSpeed = 0.02;

void Draw() {
    glUseProgram(Program); // Устанавливаем шейдерную программу текущей

    glm::mat4 view = glm::lookAt(cameraPos, cameraPos + cameraFront, cameraUp);
    glm::mat4 projection = glm::perspective(45.0f, (GLfloat)1000 / (GLfloat)1000, 0.1f, 100.0f);

    glUniformMatrix4fv(glGetUniformLocation(Program, "view"), 1, GL_FALSE, glm::value_ptr(view));
    glUniformMatrix4fv(glGetUniformLocation(Program, "projection"), 1, GL_FALSE, glm::value_ptr(projection));

    vector<glm::vec3> initial_pos = {
        {0.0f, 0.25f, 0.0f},
        {0.0f, -0.25f, 0.0f},
        {0.0f, 0.6f, 0.0f},
        {0.0f, -0.6f, 0.0f},
        {0.0f, 1.0f, 0.0f}
    };

    vector<GLfloat> bag_sizes = {
        0.01f,
        0.0075f,
        0.0175f,
        0.015f,
        0.02f
    };

    vector<GLfloat> orbit_speed_coef = {
        -0.75f,
        -0.75f,
        0.5f,
        0.5f,
        0.25f
    };

    vector<GLfloat> own_speed_coef = {
        -0.25f,
        1.0f,
        0.25f,
        0.1f,
        0.75f
    };

    vector<glm::mat4> bag_transforms;
    for (int i = 0; i < 5; i++) {
        bag_transforms.push_back(glm::mat4(1.0f));
        
        bag_transforms[i] = glm::rotate(bag_transforms[i], (GLfloat)glm::radians((clock() - start_time) * 0.1f) * orbit_speed_coef[i], glm::vec3(0.0f, 0.0f, 1.0f));
        bag_transforms[i] = glm::translate(bag_transforms[i], initial_pos[i]);
        bag_transforms[i] = glm::rotate(bag_transforms[i], (GLfloat)glm::radians((clock() - start_time) * 0.1f) * own_speed_coef[i], glm::vec3(0.0f, 0.0f, 1.0f));
        bag_transforms[i] = glm::scale(bag_transforms[i], glm::vec3(bag_sizes[i], bag_sizes[i], bag_sizes[i]));
    }


    GLuint transformLoc = glGetUniformLocation(Program, "transform");
    glUniformMatrix4fv(transformLoc, 5, GL_FALSE, glm::value_ptr(bag_transforms[0]));


    glActiveTexture(GL_TEXTURE0);
    glBindTexture(GL_TEXTURE_2D, texture1);
    glUniform1i(textureUniformID, 0);

    glBindBuffer(GL_ARRAY_BUFFER, VBO); // Подключаем VBO

    // сообщаем OpenGL как он должен интерпретировать вершинные данные.
    glEnableVertexAttribArray(coordAttribID); // Включаем массив атрибутов
    glVertexAttribPointer(coordAttribID, 3, GL_FLOAT, GL_FALSE, 20, (GLvoid*)0);
    
    glEnableVertexAttribArray(textureCoordAttribID); // Включаем массив атрибутов
    glVertexAttribPointer(textureCoordAttribID, 2, GL_FLOAT, GL_FALSE, 20, (GLvoid*)12);

    glBindBuffer(GL_ARRAY_BUFFER, 0); // Отключаем VBO

    for (int i = 0; i < 5; i++) {
        glDrawArraysInstanced(GL_TRIANGLES, 0, obj1.size(), 5); // Передаем данные на видеокарту(рисуем)
    }

    //___________________________________________________________________________

    glm::mat4 map_transform = glm::mat4(1.0f);
    map_transform = glm::scale(map_transform, glm::vec3(0.005f, 0.005f, 0.005f));
    map_transform = glm::rotate(map_transform, (GLfloat)glm::radians((clock() - start_time) * 0.1f) * 0.05f, glm::vec3(0.0f, 0.0f, 1.0f));

    transformLoc = glGetUniformLocation(Program, "transform");
    glUniformMatrix4fv(transformLoc, 1, GL_FALSE, glm::value_ptr(map_transform));

    glActiveTexture(GL_TEXTURE1);
    glBindTexture(GL_TEXTURE_2D, texture2);
    glUniform1i(textureUniformID, 1);

    glBindBuffer(GL_ARRAY_BUFFER, VBO_map);
    glVertexAttribPointer(coordAttribID, 3, GL_FLOAT, GL_FALSE, 20, (GLvoid*)0);
    glVertexAttribPointer(textureCoordAttribID, 2, GL_FLOAT, GL_FALSE, 20, (GLvoid*)12);
    glBindBuffer(GL_ARRAY_BUFFER, 0); // Отключаем VBO

    glDrawArraysInstanced(GL_TRIANGLES, 0, obj2.size(), 1); // Передаем данные на видеокарту(рисуем)

    glDisableVertexAttribArray(coordAttribID); // Отключаем массив атрибутов
    glDisableVertexAttribArray(textureCoordAttribID); // Отключаем массив атрибутов

    glUseProgram(0); // Отключаем шейдерную программу

    checkOpenGLerror();
}

// Освобождение буфера
void ReleaseVBO() {
    glBindBuffer(GL_ARRAY_BUFFER, 0);
    glDeleteBuffers(1, &VBO);
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
        
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Right)) {
            cameraFront = glm::rotate(glm::mat4(1.0f), -cameraSpeed, glm::vec3(0.0f, 1.0f, 0.0f)) * glm::vec4(cameraFront, 1.0f);
            cameraRight = glm::rotate(glm::mat4(1.0f), -cameraSpeed, glm::vec3(0.0f, 1.0f, 0.0f)) * glm::vec4(cameraRight, 1.0f);
            //cameraRight = glm::normalize(glm::cross(cameraFront, WorldUp));
        }
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Left)) {
            cameraFront = glm::rotate(glm::mat4(1.0f), cameraSpeed, glm::vec3(0.0f, 1.0f, 0.0f)) * glm::vec4(cameraFront, 1.0f);
            cameraRight = glm::rotate(glm::mat4(1.0f), cameraSpeed, glm::vec3(0.0f, 1.0f, 0.0f)) * glm::vec4(cameraRight, 1.0f);
            //cameraRight = glm::normalize(glm::cross(cameraFront, WorldUp));
        }

        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Up)) {
            cameraFront = glm::rotate(glm::mat4(1.0f), cameraSpeed, glm::vec3(1.0f, 0.0f, 0.0f)) * glm::vec4(cameraFront, 1.0f);
            cameraUp = glm::rotate(glm::mat4(1.0f), cameraSpeed, glm::vec3(1.0f, 0.0f, 0.0f)) * glm::vec4(cameraUp, 1.0f);
            //cameraUp = glm::normalize(glm::cross(cameraFront, cameraRight));
        }
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Down)) {
            cameraFront = glm::rotate(glm::mat4(1.0f), -cameraSpeed, glm::vec3(1.0f, 0.0f, 0.0f)) * glm::vec4(cameraFront, 1.0f);
            cameraUp = glm::rotate(glm::mat4(1.0f), -cameraSpeed, glm::vec3(1.0f, 0.0f, 0.0f)) * glm::vec4(cameraUp, 1.0f);
            //cameraUp = glm::normalize(glm::cross(cameraFront, cameraRight));
        }

        if (sf::Keyboard::isKeyPressed(sf::Keyboard::W)) 
            cameraPos += cameraUp * cameraSpeed;
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::S)) 
            cameraPos -= cameraUp * cameraSpeed;
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::D)) 
            cameraPos += cameraRight * cameraSpeed;
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::A)) 
            cameraPos -= cameraRight * cameraSpeed;
     
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::E)) 
            cameraPos += cameraFront * cameraSpeed;
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Q)) 
            cameraPos -= cameraFront * cameraSpeed;

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
