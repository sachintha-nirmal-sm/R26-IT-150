import unittest
import warnings

from app.services.analytics_service import actual_difficulty
from app.services.difficulty_model_service import (
    feature_vector,
    predict_difficulty,
)


class ActualDifficultyTest(unittest.TestCase):
    def test_easy_above_seventy_percent(self):
        self.assertEqual(actual_difficulty(0.71), "Easy")

    def test_medium_from_forty_through_seventy_percent(self):
        self.assertEqual(actual_difficulty(0.40), "Medium")
        self.assertEqual(actual_difficulty(0.70), "Medium")

    def test_hard_below_forty_percent(self):
        self.assertEqual(actual_difficulty(0.39), "Hard")

    def test_feature_vector_matches_training_order(self):
        features = feature_vector(
            attempts=10,
            correct_count=5,
            ability_stats={
                "low": {"attempts": 2, "correct": 0},
                "medium": {"attempts": 4, "correct": 2},
                "high": {"attempts": 4, "correct": 3},
            },
            ai_difficulty="Medium",
            diff_order={"Easy": 0, "Medium": 1, "Hard": 2},
        )
        self.assertEqual(features[:4], [0.5, 0.0, 0.5, 0.75])
        self.assertEqual(features[4], 0.75)
        self.assertEqual(features[6:], [10.0, 1.0])

    def test_saved_model_is_used_for_inference(self):
        with warnings.catch_warnings():
            warnings.simplefilter("ignore")
            label, model_name = predict_difficulty(
                attempts=11,
                correct_count=0,
                ability_stats={"low": {"attempts": 11, "correct": 0}},
                ai_difficulty="Easy",
            )
        self.assertEqual(label, "Hard")
        self.assertEqual(model_name, "Logistic Regression")


if __name__ == "__main__":
    unittest.main()
