#include <iostream>
#include <vector>
#include <set>
#include <string>
#include <random>
#include <algorithm>
#include <ctime>
#include <numeric>
using namespace std;

struct Agent {
    int id;
    set<int> target;
    set<int> patents;
    int iterations = 0;
    int rounds = 0;
    bool completed = false;
    int energy = 100;
};

bool isCompleted(const Agent& a) {
    for (int p : a.target) {
        if (a.patents.find(p) == a.patents.end()) return false;
    }
    return true;
}

int main() {
    setlocale(LC_ALL, "Rus");
    mt19937 rng(static_cast<unsigned>(time(0)));
    int n = 5;
    int patentsCount = 15;
    int targetSize = 4;

    vector<int> allPatents(patentsCount);
    iota(allPatents.begin(), allPatents.end(), 1);

    vector<Agent> agents(n);
    for (int i = 0; i < n; i++) {
        agents[i].id = i + 1;
        shuffle(allPatents.begin(), allPatents.end(), rng);
        agents[i].target.insert(allPatents.begin(), allPatents.begin() + targetSize);
    }

    set<int> unionTargets;
    for (auto& ag : agents) {
        unionTargets.insert(ag.target.begin(), ag.target.end());
    }
    vector<int> unionVec(unionTargets.begin(), unionTargets.end());

    shuffle(unionVec.begin(), unionVec.end(), rng);
    for (size_t i = 0; i < unionVec.size(); i++) {
        agents[i % n].patents.insert(unionVec[i]);
    }

    bool allDone = false;
    int iteration = 0;
    uniform_real_distribution<double> chance(0.0, 1.0);

    while (!allDone && iteration < 1000) {
        iteration++;

        for (int i = 0; i < n; i++) {
            Agent& A = agents[i];
            if (A.completed || A.energy <= 0) continue;

            int maxContacts = 2;
            for (int c = 0; c < maxContacts; c++) {
                uniform_int_distribution<int> dist(0, n - 1);
                int j = dist(rng);
                if (j == i) continue;
                Agent& B = agents[j];
                if (B.energy <= 0) continue;

                A.rounds++;
                B.rounds++;
                A.energy -= 1;
                B.energy -= 1;

                if (chance(rng) < 0.2) {
                    continue;
                }

                int foundA = -1;
                for (int p : A.target) {
                    if (A.patents.count(p) == 0 && B.patents.count(p)) {
                        foundA = p;
                        break;
                    }
                }

                if (foundA == -1) continue;

                if (B.completed) {
                    A.patents.insert(foundA);
                    cout << "Итерация " << iteration
                        << ": Агент " << A.id << " получил патент " << foundA
                        << " от Агента " << B.id << " (бесплатно)\n";
                }
                else {
                    int foundB = -1;
                    for (int p : B.target) {
                        if (B.patents.count(p) == 0 && A.patents.count(p)) {
                            foundB = p;
                            break;
                        }
                    }
                    if (foundB != -1) {

                            A.patents.insert(foundA);
                        B.patents.insert(foundB);
                        cout << "Итерация " << iteration
                            << ": Агент " << A.id << " получил " << foundA
                            << " <-> " << "Агент " << B.id << " получил " << foundB << "\n";
                    }
                }
            }
        }

        allDone = true;
        for (auto& ag : agents) {
            if (!ag.completed && isCompleted(ag)) {
                ag.completed = true;
                ag.iterations = iteration;
                cout << ">>> Агент " << ag.id << " завершил сбор набора на итерации " << iteration << "\n";
            }
            if (!ag.completed) allDone = false;
        }
    }

    cout << "\nРезультаты моделирования:\n";
    for (auto& ag : agents) {
        cout << "Агент " << ag.id
            << " | целевой набор: " << ag.target.size()
            << " | итерации: " << ag.iterations
            << " | раунды коммуникаций: " << ag.rounds
            << " | энергия: " << ag.energy
            << "\n";
    }
}